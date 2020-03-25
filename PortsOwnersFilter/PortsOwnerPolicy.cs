using CommonStandard;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PortsOwnersFilter
{
    public class PortsOwnerPolicy : JSONBaseClass, IPortsOwnerPolicy
    {
        HashSet<int> TrustedProcessIds = new HashSet<int>();

        FilterMode PolicyMode = FilterMode.Whitelist;

        [JsonIgnore]
        Dictionary<OwnerType, HashSet<string>> __trustedUsersSearch
            = new Dictionary<OwnerType, HashSet<string>>();
        [JsonIgnore]
        List<UserOwner> __trustedUsersList = new List<UserOwner>();

        public IList<UserOwner> TrustedUsers
        {
            get { return __trustedUsersList.AsReadOnly(); }
            set {
                __trustedUsersList = new List<UserOwner>(value);
                __trustedUsersSearch = new Dictionary<OwnerType, HashSet<string>>();
                __trustedUsersSearch.Add(OwnerType.NAME, new HashSet<string>());
                __trustedUsersSearch.Add(OwnerType.SID, new HashSet<string>());

                foreach (var item in __trustedUsersList)
                {
                    __trustedUsersSearch[item.Type].Add(item.Value);
                }
            }
        }


        [JsonIgnore]
        Dictionary<OwnerType, HashSet<string>> __trustedGroupsSearch
            = new Dictionary<OwnerType, HashSet<string>>();
        [JsonIgnore]
        List<GroupOwner> __trustedGroupsList = new List<GroupOwner>();

        public IList<GroupOwner> TrustedGroups
        {
            get { return __trustedGroupsList.AsReadOnly(); }
            set
            {
                __trustedGroupsList = new List<GroupOwner>(value);
                __trustedGroupsSearch = new Dictionary<OwnerType, HashSet<string>>();
                __trustedGroupsSearch.Add(OwnerType.NAME, new HashSet<string>());
                __trustedGroupsSearch.Add(OwnerType.SID, new HashSet<string>());

                foreach (var item in __trustedGroupsList)
                {
                    __trustedGroupsSearch[item.Type].Add(item.Value);
                }
            }
        }

        [JsonIgnore]
        Dictionary<string, PathPolicy> __allowedPathSearch = new Dictionary<string, PathPolicy>();
        [JsonIgnore]
        List<PathPolicy> __allowedPaths = new List<PathPolicy>();
        public IList<PathPolicy> AllowedPaths
        {
            get { return __allowedPaths.AsReadOnly(); }
            set
            {
                __allowedPaths = new List<PathPolicy>(value);
                foreach (var item in __allowedPaths)
                {
                    if (!__allowedPathSearch.ContainsKey(item.Path)) // we dont trust the input to be de-duplicated
                        __allowedPathSearch.Add(item.Path, item);
                }
            }
        }

        // ========================= Implementation

        public void addTrustedPid(int pid)
        {
            TrustedProcessIds.Add(pid);
        }

        public void removeTrustedPid(int pid)
        {
            TrustedProcessIds.Remove(pid);
        }

        public void setMode(FilterMode mode)
        {
            PolicyMode = mode;
        }

        public FilterMode getMode()
        {
            return PolicyMode;
        }

        public bool isTrustedPid(int pid)
        {
            return TrustedProcessIds.Contains(pid);
        }

        public bool isUserTrusted(UserOwner user)
        {
            bool foundAllowed = false;

            if (__trustedUsersSearch[user.Type].Contains(user.Value))
            {
                foundAllowed = true;
            }

            return foundAllowed;
        }

        public bool isGroupsTrusted(GroupOwner[] usergroups)
        {
            bool foundAllowed = false;
            foreach(var item in usergroups)
            {
                if (__trustedGroupsSearch[item.Type].Contains(item.Value))
                {
                    foundAllowed = true;
                    break;
                }
            }
            return foundAllowed;
        }

        public bool isProcessPathAllowed(string fullpath, UserOwner user, GroupOwner[] usergroups)
        {
            string myPath = PathPolicy.makeStandartPath(fullpath);
            if (!__allowedPathSearch.ContainsKey(myPath))
            {
                return false;
            }

            bool found = false;
            if (!found)
            {
                found = __allowedPathSearch[myPath].AllowedUsers.Contains(user);
            }
            if (!found)
            {
                foreach (var item in usergroups)
                {
                    if (__allowedPathSearch[myPath].AllowedGroups.Contains(item))
                    {
                        found = true;
                        break;
                    }
                }
            }
            
            return found;
        }

        public bool isAllowed(int pid, string fullpath, UserOwner user, GroupOwner[] usergroups)
        {
            return isTrustedPid(pid) ||
                isUserTrusted(user) ||
                isGroupsTrusted(usergroups) ||
                isProcessPathAllowed(fullpath, user, usergroups);
        }


        // ========================= Save & Load

        public void reloadPolicy(string jsonContent)
        {
            PortsOwnerPolicy newPolicy = JSONBaseClass.FromJSONString<PortsOwnerPolicy>(jsonContent, defValue: null);
            if (newPolicy != null)
            {
                TrustedProcessIds = newPolicy.TrustedProcessIds;

                PolicyMode = newPolicy.PolicyMode;
                TrustedUsers = newPolicy.TrustedUsers;
                TrustedGroups = newPolicy.TrustedGroups;
                AllowedPaths = newPolicy.AllowedPaths;
            }
        }

        public string savePolicy()
        {
            return ToJSON();
        }


    }
}
