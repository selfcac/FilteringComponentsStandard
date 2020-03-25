using System;
using System.Collections.Generic;
using System.Text;

namespace PortsOwnersFilter
{
    public class PortsOwnerPolicy : IPortsOwnerPolicy
    {
        List<int> TrustedProcessIds = new List<int>();

        FilterMode PolicyMode = FilterMode.Whitelist;
        List<UserOwner> TrustedUsers = new List<UserOwner>();
        List<GroupOwner> TrustedGroups = new List<GroupOwner>();
        List<PathPolicy> AllowedPaths = new List<PathPolicy>();

        // ========================= Implementation

        public void addTrustedPid(int pid)
        {
            throw new NotImplementedException();
        }

        public FilterMode getMode()
        {
            throw new NotImplementedException();
        }

        public bool isAllowed(int pid, string fullpath, UserOwner user, GroupOwner[] groups)
        {
            throw new NotImplementedException();
        }

        public bool isGroupsTrusted(GroupOwner[] groups)
        {
            throw new NotImplementedException();
        }

        public bool isProcessPathAllowed(string fullpath, UserOwner user, GroupOwner[] groups)
        {
            throw new NotImplementedException();
        }

        public void isTrustedPid(int pid)
        {
            throw new NotImplementedException();
        }

        public bool isUserTrusted(UserOwner user)
        {
            throw new NotImplementedException();
        }

        public void setMode(FilterMode mode)
        {
            throw new NotImplementedException();
        }

        public void reloadPolicy(string jsonContent)
        {
            throw new NotImplementedException();
        }

        public string savePolicy()
        {
            throw new NotImplementedException();
        }
    }
}
