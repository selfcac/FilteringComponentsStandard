using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckBlacklistedWifi
{
    public class WifiHelper
    {
        public class WifiNetwork
        {
            public enum TrustMode
            {
                TRUSTED, IGNORED, BLOCKED, UNKOWN
            }

            public static char seperator = ';';

            public static Dictionary<string, TrustMode> TrustMap = new Dictionary<string, TrustMode>()
            {
                { "+", TrustMode.TRUSTED },
                { "?", TrustMode.IGNORED},
                { "-", TrustMode.BLOCKED },
                { "*", TrustMode.UNKOWN },
            };

            public static Dictionary<TrustMode, string> ReverseTrustMap = new Dictionary<TrustMode, string>()
            {
                {TrustMode.TRUSTED, "+" },
                {TrustMode.IGNORED, "?" },
                {TrustMode.BLOCKED, "-" },
                {TrustMode.UNKOWN,  "*" },
            };

            public string BSSID;
            public string DisplaName;
            public TrustMode TrustLevel;

            public static WifiNetwork fromString(string wifidata, Action<string> logger, out bool error)
            {
                error = true;
                WifiNetwork result = new WifiNetwork();
                try
                {
                    List<string> data =
                        wifidata.Split(seperator).Select((a) => a.Trim()).ToList();
                    if (data.Count == 3)
                    {
                        result.TrustLevel = TrustMap[data[0]];
                        result.BSSID = data[1];
                        result.DisplaName = data[2];
                        error = false;
                    }
                }
                catch (Exception ex)
                {
                    logger(ex.ToString());
                }
                return result;
            }

            public static WifiNetwork newNetwork(string bssid, string name)
            {
                return new WifiNetwork() { TrustLevel = TrustMode.UNKOWN, BSSID = bssid, DisplaName = name };
            }


            public override int GetHashCode() 
            {
                // Only for bucket search ==> after that must use Equls.
                return BSSID.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return GetHashCode() == obj.GetHashCode();
            }

            public override string ToString()
            {
                return string.Join(
                    seperator + "",
                    new[] { ReverseTrustMap[TrustLevel], BSSID, DisplaName }
                    );
            }
        }

        /// <summary>
        /// Main decision tree to see if we are in block zone.
        /// </summary>
        /// <param name="currentIDs">Wifi BSSID that currently around us</param>
        /// <param name="badIDs">Wifi BSSID that we found in bad zones</param>
        /// <param name="ignoreIDs">Wifi BSSID that are portable (like your phone hotspot)</param>
        /// <param name="trustedIDs">Wifi BSSID that will only be available in trusted zone</param>
        /// <param name="newBadIDs">A output of new wifi BSSID to block if we are in block zone</param>
        /// <param name="log">A logging function</param>
        /// <returns></returns>
        public static bool inBlockZone(
            IEnumerable<WifiNetwork> currentIDs,
            IEnumerable<WifiNetwork> badIDs, IEnumerable<WifiNetwork> ignoreIDs, IEnumerable<WifiNetwork> trustedIDs,
            out List<WifiNetwork> newBadIDs, Action<string> log)
        {
            bool inblockzone = true; // until proven innocent

            // Make hashset for faster searching:
            HashSet<WifiNetwork> currentHashes = new HashSet<WifiNetwork>(currentIDs),
                badsHashes = new HashSet<WifiNetwork>(badIDs),
                ignoredHashes = new HashSet<WifiNetwork>(ignoreIDs),
                trustedHashes = new HashSet<WifiNetwork>(trustedIDs);

            newBadIDs = new List<WifiNetwork>();

            HashSet<WifiNetwork> relevantHashes = new HashSet<WifiNetwork>();

            bool foundTrusted = false;
            int ignoredCount = 0;

            // Find relevant ids. If you find trusted, just stop.
            foreach (WifiNetwork id in currentHashes)
            {
                if (trustedHashes.Contains(id))
                {
                    log("Found trusted ('" + id + "'). So not in blockzone.");
                    foundTrusted = true;
                    break;
                }
                else if (ignoredHashes.Contains(id))
                {
                    // Dont add it to next step
                    ignoredCount++;
                }
                else
                {
                    // not trusted and not ignored --> so need to be checked
                    relevantHashes.Add(id);
                }
            }

            if (foundTrusted)
            {
                inblockzone = false;
            }
            else
            {
                bool foundBlocked = false;

                // Now check all relevant:
                foreach (WifiNetwork id in relevantHashes)
                {
                    if (badsHashes.Contains(id))
                    {
                        if (!foundBlocked) // log once
                            log("Found first blocked BSSID ('" + id + "').");
                        foundBlocked = true;
                        // Don't break loop! we still want to know every new!
                    }
                    else
                    {
                        // We already filtered ignored, so you it is new and suspected to be in
                        //      block zone:
                        newBadIDs.Add(id);
                    }
                }

                if (foundBlocked)
                {
                    inblockzone = true;
                    log("In blockzone. Found " + newBadIDs.Count + " new BSSID.");
                }
                else
                {
                    inblockzone = false;
                    newBadIDs.Clear(); // no need if we are outside of black zone.
                    log("Not in blockzone. Found " + relevantHashes.Count + " BSSID around.");
                }
            }

            return inblockzone;
        }

        /// <summary>
        /// Quick call of the inBlockZone functions
        /// </summary>
        /// <param name="textcurrent">List of current Wifi BSSID</param>
        /// <param name="textrules">prefix: '-' block, '+' trusted, '?' ignore</param>
        /// <param name="newBSSIDsRules">the entire new rule set (not only the added blocked)</param>
        /// <param name="log">A logging function</param>
        /// <returns></returns>
        public static bool fastBlockZoneCheck(
            IEnumerable<string> textcurrent,
            List<string> textrules,
            Action<string> log)
        {
            bool inBlockZone = true;

            try
            {
                List<WifiNetwork> networkRules = new List<WifiNetwork>();
                foreach (string rule in textrules)
                {
                    bool error = false;
                    WifiNetwork wifi = WifiNetwork.fromString(rule, log, out error);
                    if (!error)
                        networkRules.Add(wifi);
                }

                List<WifiNetwork> currentWifis = textcurrent
                    .Select((t) =>
                    {
                        var data = t
                            .Split(WifiNetwork.seperator)
                            .Select((a) => a.Trim())
                            .ToList();
                        return WifiNetwork.newNetwork(data[0], data[1]);
                    })
                    .ToList();

                List<WifiNetwork> trusted = networkRules
                    .Where((s) => s.TrustLevel == WifiNetwork.TrustMode.TRUSTED).ToList();

                List<WifiNetwork> ignored = networkRules
                    .Where((s) => s.TrustLevel == WifiNetwork.TrustMode.IGNORED).ToList();

                List<WifiNetwork> blocked = networkRules
                    .Where((s) => s.TrustLevel == WifiNetwork.TrustMode.BLOCKED).ToList();

                List<WifiNetwork> newBadWifis = new List<WifiNetwork>();

                inBlockZone = WifiHelper.inBlockZone(
                    currentWifis,
                    blocked, ignored, trusted, out newBadWifis, log);

                if (inBlockZone)
                {
                    foreach (var newbad in newBadWifis)
                    {
                        newbad.TrustLevel = WifiNetwork.TrustMode.BLOCKED; // From unkown to bad.
                        textrules.Add(newbad.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                log(ex.ToString());
            }

            return inBlockZone;
        }

    }
}
