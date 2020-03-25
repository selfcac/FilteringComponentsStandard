using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortsOwnersFilter;

namespace PortsOwnersFilter_Tests
{
    [TestClass]
    public class UnitTest1
    {
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
            Assert.AreEqual(filter.getPathFilterMode(), FilterMode.Whitelist);
            filter.setPathFilterMode(FilterMode.Blacklist);
            Assert.AreEqual(filter.getPathFilterMode(), FilterMode.Blacklist);

            // Todo: check inverting of result
        }
    }
}
