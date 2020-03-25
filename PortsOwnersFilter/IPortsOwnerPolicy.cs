using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortsOwnersFilter
{
    public enum OwnerType
    {
        NAME= 0,
        SID,
    }

    public enum PathAllowedType
    {
        GroupOrUsers,
        ANY
    }

    public enum FilterMode
    {
        // Only allow bypass if match the policy
        Whitelist,

        // Only force filter if match the policy
        //      trusted pid are still always bypass
        Blacklist,
    }

    // S-1-5-32-544	Admin

    public class UserOwner
    {
        public OwnerType Type;
        public string Value;

        public override bool Equals(object obj)
        {
            UserOwner other = obj as UserOwner;
            return other != null && other.Type == Type && other.Value == Value;
        }
    }

    public class GroupOwner
    {
        public OwnerType Type;
        public string Value;

        public override bool Equals(object obj)
        {
            GroupOwner other = obj as GroupOwner;
            return other != null && other.Type == Type && other.Value == Value;
        }
    }

    public class PathPolicy
    {
        [JsonIgnore]
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = makeStandartPath(value);
            }
        }

        public PathAllowedType PathFilter;

        public List<UserOwner> AllowedUsers = new List<UserOwner>();
        public List<GroupOwner> AllowedGroups = new List<GroupOwner>();

        public static string makeStandartPath(string path)
        {
            return path.ToLower();
        }

    }

    // you get pid 
    //  -> you get {Username, ProcessPath} 
    //  -> you get {Username, [User.Groups], ProcessPath}
    public interface IPortsOwnerPolicy
    {
        FilterMode getPathFilterMode();
        void setPathFilterMode(FilterMode mode);

        // You can remember trusted process ids (like the host process of this filter etc.)
        void addTrustedPid(int pid);
        void removeTrustedPid(int pid);
        bool isTrustedPid(int pid);

        // You can ignore Path if user is trusted
        bool isUserTrusted(UserOwner user);

        // You can ignore Path if user in group that is trusted
        bool isGroupsTrusted(GroupOwner[] usergroups);

        // Finally you check if path is allowed (with [group || user || any user] combined)
        bool isProcessPathAllowed(string fullpath, UserOwner user, GroupOwner[] usergroups);

        // Check all (pid, trusted user, trusted group, path with those user\groups)
        bool isAllowed(int pid, string fullpath, UserOwner user, GroupOwner[] usergroups);

        void reloadPolicy(string jsonContent);
        string savePolicy();
    }
}
