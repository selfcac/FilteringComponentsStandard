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
        OwnerType Type;
        string Value;
    }

    public class GroupOwner
    {
        OwnerType Type;
        string Value;
    }

    public class PathPolicy
    {
        string Path;
        PathAllowedType PathType;

        UserOwner[] AllowedUsers;
        GroupOwner[] AllowedGroups;
    }

    // you get pid 
    //  -> you get {Username, ProcessPath} 
    //  -> you get {Username, [User.Groups], ProcessPath}
    public interface IPortsOwnerPolicy
    {
        FilterMode getMode();
        void setMode(FilterMode mode);

        // You can remember trusted process ids (like the host process of this filter etc.)
        void addTrustedPid(int pid);
        void isTrustedPid(int pid);

        // You can ignore Path if user is trusted
        bool isUserTrusted(UserOwner user);

        // You can ignore Path if user in group that is trusted
        bool isGroupsTrusted(GroupOwner[] groups);

        // Finally you check if path is allowed (with group, user, any user tuple)
        bool isProcessPathAllowed(string fullpath, UserOwner user, GroupOwner[] groups);

        // Check all (pid, trusted user, trusted group, path with those user\groups)
        bool isAllowed(int pid, string fullpath, UserOwner user, GroupOwner[] groups);
    }
}
