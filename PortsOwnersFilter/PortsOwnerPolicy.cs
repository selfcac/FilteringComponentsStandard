using CommonStandard;
using System;
using System.Collections.Generic;
using System.Text;

namespace PortsOwnersFilter
{
    public class PortsOwnerPolicy : JSONBaseClass, IPortsOwnerPolicy
    {
        HashSet<int> TrustedProcessIds = new HashSet<int>();

        FilterMode PolicyMode = FilterMode.Whitelist;
        List<UserOwner> TrustedUsers = new List<UserOwner>();
        List<GroupOwner> TrustedGroups = new List<GroupOwner>();
        List<PathPolicy> AllowedPaths = new List<PathPolicy>();

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

        public bool isGroupsTrusted(GroupOwner[] groups)
        {
            throw new NotImplementedException();
        }

        public bool isProcessPathAllowed(string fullpath, UserOwner user, GroupOwner[] groups)
        {
            throw new NotImplementedException();
        }

        public bool isTrustedPid(int pid)
        {
            return TrustedProcessIds.Contains(pid);
        }

        public bool isUserTrusted(UserOwner user)
        {
            throw new NotImplementedException();
        }

        public bool isAllowed(int pid, string fullpath, UserOwner user, GroupOwner[] groups)
        {
            throw new NotImplementedException();
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
