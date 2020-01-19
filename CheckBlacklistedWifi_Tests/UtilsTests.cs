using CheckBlacklistedWifiStandard;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CheckBlacklistedWifi_Tests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod()]
        public void getWifiParsedTest()
        {
            string Input = @"    
SSID 66 : Moshe
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 7               : 8c:59:c3:aa:1d:dc
         Signal             : 92%
         Radio type         : 802.11n
         Channel            : 11
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54
    BSSID 12             : 8c:59:c3:aa:1d:e0
         Signal             : 34%
         Radio type         : 802.11ac
         Channel            : 36
         Basic rates (Mbps) : 6 12 24
         Other rates (Mbps) : 9 18 36 48 54";

            List<string> currentWifis = Utils.getWifiWindowsCMDParsed(Input);
            Assert.AreEqual(2, currentWifis.Count);
            Assert.AreEqual("8c:59:c3:aa:1d:dc;Moshe", currentWifis[0]);
            Assert.AreEqual("8c:59:c3:aa:1d:e0;Moshe", currentWifis[1]);


        }

        [TestMethod()]
        public void getWifiParsedTest2()
        {
            string Input = @"SSID 1 : yoyoyo
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : d0:0f:6d:c1:66:97
         Signal             : 100%
         Radio type         : 802.11n
         Channel            : 9
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 2 : Partner-4G-AE5A
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : a8:c8:3a:35:ae:5a
         Signal             : 70%
         Radio type         : 802.11n
         Channel            : 5
         Basic rates (Mbps) : 1 2
         Other rates (Mbps) : 5.5 6 9 11 12 18 24 36 48 54

SSID 3 : netta
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : a0:ab:1b:55:b9:6c
         Signal             : 64%
         Radio type         : 802.11n
         Channel            : 3
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 4 : vaknin
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : 00:8c:54:8a:d7:91
         Signal             : 100%
         Radio type         : 802.11n
         Channel            : 1
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 5 : asaf
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : ac:3b:77:58:fa:24
         Signal             : 100%
         Radio type         : 802.11n
         Channel            : 1
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 6 : Kamal.S
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : 40:9b:cd:d0:40:1e
         Signal             : 96%
         Radio type         : 802.11n
         Channel            : 1
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 7 : Moshe
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : 8c:59:c3:aa:1d:e0
         Signal             : 32%
         Radio type         : 802.11ac
         Channel            : 36
         Basic rates (Mbps) : 6 12 24
         Other rates (Mbps) : 9 18 36 48 54
    BSSID 2                 : 8c:59:c3:aa:1d:dc
         Signal             : 90%
         Radio type         : 802.11n
         Channel            : 1
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 8 : asaf5
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : ac:3b:77:58:fa:25
         Signal             : 56%
         Radio type         : 802.11ac
         Channel            : 36
         Basic rates (Mbps) : 6 12 24
         Other rates (Mbps) : 9 18 36 48 54

SSID 9 : Kamal.S_5
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : 40:9b:cd:d0:40:1d
         Signal             : 26%
         Radio type         : 802.11ac
         Channel            : 64
         Basic rates (Mbps) : 6 12 24
         Other rates (Mbps) : 9 18 36 48 54

SSID 10 : itai
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : 10:be:f5:38:76:3a
         Signal             : 60%
         Radio type         : 802.11n
         Channel            : 9
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 11 : Asaf
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : a0:ab:1b:5b:12:7c
         Signal             : 68%
         Radio type         : 802.11n
         Channel            : 11
         Basic rates (Mbps) : 1 2 5.5 11
         Other rates (Mbps) : 6 9 12 18 24 36 48 54

SSID 12 : DIRECT-22-HP OfficeJet Pro 8710
    Network type            : Infrastructure
    Authentication          : WPA2-Personal
    Encryption              : CCMP
    BSSID 1                 : c8:d3:ff:89:2a:25
         Signal             : 62%
         Radio type         : 802.11n
         Channel            : 6
         Basic rates (Mbps) : 6 12 24
         Other rates (Mbps) : 9 18 36 48 54";

            string output = @"d0:0f:6d:c1:66:97;yoyoyo
a8:c8:3a:35:ae:5a;Partner-4G-AE5A
a0:ab:1b:55:b9:6c;netta
00:8c:54:8a:d7:91;vaknin
ac:3b:77:58:fa:24;asaf
40:9b:cd:d0:40:1e;Kamal.S
8c:59:c3:aa:1d:e0;Moshe
8c:59:c3:aa:1d:dc;Moshe
ac:3b:77:58:fa:25;asaf5
40:9b:cd:d0:40:1d;Kamal.S_5
10:be:f5:38:76:3a;itai
a0:ab:1b:5b:12:7c;Asaf
c8:d3:ff:89:2a:25;DIRECT-22-HP OfficeJet Pro 8710".Replace("\r", "");

            List<string> currentWifis = Utils.getWifiWindowsCMDParsed(Input);
            Assert.AreEqual(13, currentWifis.Count);
            Assert.AreEqual(output, string.Join("\n", currentWifis));


        }
    }
}
