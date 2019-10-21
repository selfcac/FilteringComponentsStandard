using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckBlacklistedWifi
{
    public class Utils
    {
        public static List<string> getWifiParsed(string cmdData)
        {
            List<string> result = new List<string>();
            string lastName = "Empty Name";
            foreach(string row in cmdData.Split('\n'))
            {
                int indexOfNekudotaim = row.IndexOf(':');
                if (indexOfNekudotaim > -1 && indexOfNekudotaim < row.Length -2)
                {
                    string[] data = new string[] {
                        row.Substring(0,indexOfNekudotaim),
                        row.Substring(indexOfNekudotaim+1)
                    };
                    // Clear spaces:
                    string key = data[0].Trim().ToLower();
                    string value = data[1].Trim();

                    // Find data:
                    if (key.StartsWith("ssid"))
                        lastName = value;
                    else if (key.StartsWith("bssid"))
                        result.Add(value + WifiHelper.WifiNetwork.seperator + lastName);
                }
            }
            return result;
        }
    }
}
