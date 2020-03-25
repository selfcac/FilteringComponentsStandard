using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortsOwnersFilter;
using System.Collections.Generic;

namespace PortsOwnersFilter_Tests
{
    [TestClass]
    public class UnitTest1
    {
        static UserOwner makeUserOwner(string name, OwnerType type = OwnerType.NAME)
        {
            return new UserOwner()
            {
                Type = OwnerType.NAME,
                Value = name
            };
        }

        [TestMethod]
        public void CheckTrustedPid()
        {
            PortsOwnerPolicy filter = new PortsOwnerPolicy();
            filter.addTrustedPid(123);

            Assert.IsTrue(filter.isTrustedPid(123));

            filter.removeTrustedPid(123);
            Assert.IsFalse(filter.isTrustedPid(123));
        }
        
        [TestMethod]
        public void CheckFilterMode()
        {
            PortsOwnerPolicy filter = new PortsOwnerPolicy();
            filter.AllowedPaths = new List<PathPolicy>()
            {
                new PathPolicy()
                {
                    Path = "app.exe",
                    PathFilter = PathAllowedType.GroupOrUsers,
                    AllowedUsers = new List<UserOwner>()
                    {
                       makeUserOwner("User")
                    }
                }
            };

            Assert.AreEqual(filter.getPathFilterMode(), FilterMode.Whitelist);
            Assert.IsFalse(filter
                .isProcessPathAllowed("not-in-policy", makeUserOwner("Unknown"), new GroupOwner[0]));
            Assert.IsFalse(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("Unknown"), new GroupOwner[0]));
            Assert.IsTrue(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("User"), new GroupOwner[0]));

            filter.setPathFilterMode(FilterMode.Blacklist);
            Assert.AreEqual(filter.getPathFilterMode(), FilterMode.Blacklist);
            Assert.IsTrue(filter
                .isProcessPathAllowed("not-in-policy", makeUserOwner("Unknown"), new GroupOwner[0]));
            Assert.IsTrue(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("Unknown"), new GroupOwner[0]));
            Assert.IsFalse(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("User"), new GroupOwner[0]));

        }


    }
}
