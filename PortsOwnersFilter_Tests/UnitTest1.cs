using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortsOwnersFilter;
using System.Collections.Generic;

namespace PortsOwnersFilter_Tests
{
    [TestClass]
    public class UnitTest1
    {
        static GroupOwner[] emptyGroups = new GroupOwner[0];
        static UserOwner emptyUser = makeUserOwner("");

        static UserOwner makeUserOwner(string name, OwnerType type = OwnerType.NAME)
        {
            return new UserOwner()
            {
                Type = type,
                Value = name
            };
        }

        static GroupOwner makeGroupOwner(string name, OwnerType type = OwnerType.NAME)
        {
            return new GroupOwner()
            {
                Type = type,
                Value = name
            };
        }

        [TestMethod]
        public void CheckTrustedPid()
        {
            PortsOwnerPolicy filter = new PortsOwnerPolicy();
            filter.addTrustedPid(123);

            Assert.IsTrue(filter.isTrustedPid(123));
            Assert.IsTrue(filter.isAllowed(123,"", emptyUser, emptyGroups));

            filter.removeTrustedPid(123);
            Assert.IsFalse(filter.isTrustedPid(123));
            Assert.IsFalse(filter.isAllowed(123, "", emptyUser, emptyGroups));
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
                .isProcessPathAllowed("not-in-policy", makeUserOwner("Unknown"), emptyGroups));
            Assert.IsFalse(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("Unknown"), emptyGroups));
            Assert.IsTrue(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("User"), emptyGroups));

            filter.setPathFilterMode(FilterMode.Blacklist);
            Assert.AreEqual(filter.getPathFilterMode(), FilterMode.Blacklist);
            Assert.IsTrue(filter
                .isProcessPathAllowed("not-in-policy", makeUserOwner("Unknown"), emptyGroups));
            Assert.IsTrue(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("Unknown"), emptyGroups));
            Assert.IsFalse(filter
                .isProcessPathAllowed("app.exe", makeUserOwner("User"), emptyGroups));
        }

        [TestMethod] 
        public void checkAllow()
        {
            PortsOwnerPolicy filter = new PortsOwnerPolicy()
            {
                TrustedUsers = new List<UserOwner>()
                {
                    makeUserOwner("trusted-u-name1"),
                    makeUserOwner("trusted-u-name2"),
                    makeUserOwner("trusted-u-sid1", OwnerType.SID),
                    makeUserOwner("trusted-u-sid2", OwnerType.SID)
                },
                TrustedGroups = new List<GroupOwner>()
                {
                    makeGroupOwner("trusted-g-name1"),
                    makeGroupOwner("trusted-g-name2"),
                    makeGroupOwner("trusted-g-sid1", OwnerType.SID),
                    makeGroupOwner("trusted-g-sid2", OwnerType.SID)
                },
                AllowedPaths = new List<PathPolicy>()
                {
                    new PathPolicy()
                    {
                         Path = "AlwaysBlocked",
                         PathFilter = PathAllowedType.GroupOrUsers
                    },
                    new PathPolicy()
                    {
                        Path = "AllUsers",
                        PathFilter = PathAllowedType.ANY
                    },
                    new PathPolicy()
                    {
                        Path = "app.exe",
                        PathFilter = PathAllowedType.GroupOrUsers,
                        AllowedUsers = new List<UserOwner>()
                        {
                            makeUserOwner("app-u-1"),
                            makeUserOwner("app-u-2"),
                            makeUserOwner("app-u-1-sid", OwnerType.SID),
                            makeUserOwner("app-u-2-sid", OwnerType.SID),
                        },
                        AllowedGroups = new List<GroupOwner>()
                        {
                            makeGroupOwner("app-g-1"),
                            makeGroupOwner("app-g-2"),
                            makeGroupOwner("app-g-1-sid", OwnerType.SID),
                            makeGroupOwner("app-g-2-sid", OwnerType.SID),
                        }
                    }
                }
            };
            string json = filter.savePolicy();

            Assert.IsFalse(filter.isAllowed(0, "unknown", makeUserOwner("un-trusted-u-name1"), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "unknown", makeUserOwner("trusted-u-name1"), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "unknown", makeUserOwner("trusted-u-name2"), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "unknown", makeUserOwner("trusted-u-sid1", OwnerType.SID), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "unknown", makeUserOwner("trusted-u-sid2", OwnerType.SID), emptyGroups));

            Assert.IsFalse(filter.isAllowed(0, "unknown", emptyUser, new GroupOwner[] { makeGroupOwner("untrusted") }));
            Assert.IsTrue(filter.isAllowed(0, "unknown", emptyUser, new GroupOwner[] { makeGroupOwner("trusted-g-name1") }));
            Assert.IsTrue(filter.isAllowed(0, "unknown", emptyUser, new GroupOwner[] { makeGroupOwner("trusted-g-name2") }));
            Assert.IsTrue(filter.isAllowed(0, "unknown", emptyUser, new GroupOwner[] { makeGroupOwner("trusted-g-sid1", OwnerType.SID) }));
            Assert.IsTrue(filter.isAllowed(0, "unknown", emptyUser, new GroupOwner[] { makeGroupOwner("trusted-g-sid2", OwnerType.SID) }));

            Assert.IsFalse(filter.isAllowed(0, "AlwaysBlocked", makeUserOwner("app-u-1"), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "AllUsers", makeUserOwner("app-u-1"), emptyGroups));

            Assert.IsFalse(
                filter.isAllowed(0, "aPp.ExE", makeUserOwner("unknown-u"), new GroupOwner[] { makeGroupOwner("unknown-g") }));

            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", makeUserOwner("app-u-1"), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", makeUserOwner("app-u-2"), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", makeUserOwner("app-u-1-sid", OwnerType.SID), emptyGroups));
            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", makeUserOwner("app-u-2-sid", OwnerType.SID), emptyGroups));

            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", emptyUser, new GroupOwner[] { makeGroupOwner("app-g-1"), }));
            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", emptyUser, new GroupOwner[] { makeGroupOwner("app-g-2"),}));
            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", emptyUser, new GroupOwner[] { makeGroupOwner("app-g-1-sid", OwnerType.SID),}));
            Assert.IsTrue(filter.isAllowed(0, "aPp.ExE", emptyUser, new GroupOwner[] { makeGroupOwner("app-g-2-sid", OwnerType.SID), }));
        }


    }
}
