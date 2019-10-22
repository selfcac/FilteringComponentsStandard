using CheckBlacklistedWifi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CheckBlacklistedWifi_Tests
{
    [TestClass()]
    public class WifiHelperTests
    {
        public List<string> TestWifiBlocked(bool inBlockZone, List<string> current, List<string> rules)
        {
            List<string> newRules;
            Assert.AreEqual(inBlockZone, WifiHelper.fastBlockZoneCheck(
               current, rules, out newRules, (s) => Console.WriteLine(s)
                ));
            return newRules;
        }

        [TestMethod()]
        public void simpleBlockZone()
        {
            List<string> current = new List<string>()
            {
                "hash1;Name of wifi",
                "hash2;Name1 of wifi",
                "hash3;Name of2 wifi",
                "hash4    ;          Name of wifi3       ",
            };

            var rules = new List<string>()
            {
                "-    ;    hash1    ;                Name ignored"
            };

            var newRules = TestWifiBlocked(true, current, rules);

            // Check hashes in new rules:
            Assert.AreEqual(4, newRules.Count);
            Assert.AreEqual("-;hash4;Name of wifi3", newRules[3]);
        }

        [TestMethod()]
        public void simpleTrustedZone()
        {
            List<string> current = new List<string>()
            {
                "hash1;Name of wifi",
                "hash2;Name1 of wifi",
                "hash3;Name of2 wifi",
                "hash4;Name of wifi3",
            };

            var rules = new List<string>()
            {
                "+;hash1;Name ignored",
                "-;hash2;Name ignored"
            };

            var newRules = TestWifiBlocked(false, current, rules);

            // Check hashes in new rules:
            Assert.IsTrue(newRules.Count == 2);
        }

        [TestMethod()]
        public void simpleIgnore()
        {
            List<string> current = new List<string>()
            {
                "hash1;Name of wifi",
                "hash5;Name of wifi3",
            };

            var rules = new List<string>()
            {
                "?;hash1;Name ignored",
                "-;hash2;Name ignored"
            };

            var rules2 = new List<string>()
            {
                "-;hash1;Name ignored",
                "-;hash2;Name ignored"
            };

            var newRules = TestWifiBlocked(false, current, rules);

            // Check hashes in new rules:
            Assert.IsTrue(newRules.Count == 2);

            TestWifiBlocked(true, current, rules2);
        }


        [TestMethod()]
        public void nutralZone()
        {
            List<string> current = new List<string>()
            {
                "hash1;Name of wifi",
                "hash2;Name1 of wifi",
                "hash3;Name of2 wifi",
                "hash4;Name of wifi3",
            };

            var rules = new List<string>()
            {
                "-;hash55;Name ignored"
            };

            var newRules = TestWifiBlocked(false, current, rules);

            // Check hashes in new rules:
            Assert.AreEqual(1, newRules.Count);
            Assert.AreEqual("-;hash55;Name ignored", newRules[0]);
        }
    }
}
