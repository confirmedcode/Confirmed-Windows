using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrustWindowsWPF
{
    class ConfirmedConstants
    {
        public static int versionNumber
        {
            get
            {
                if (Properties.Settings.Default.apiVersion == version3String)
                {
                    return 3;
                }
                else if(Properties.Settings.Default.apiVersion == version2String)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
        }

        public static string serverAddress
        {
            get
            {
                if (versionNumber == 3)
                {
                    return "confirmedvpn.com"; //"trusty-ap.science";
                }
                else
                {
                    return "confirmedvpn.co";
                }
            }
        }
            

        public static string apiAddress
        {
            get
            {
                switch(versionNumber)
                {
                    case 1:
                        return "https://v1." + serverAddress;
                    case 2:
                        return "https://v2." + serverAddress;
                    case 3:
                    default:
                        return "https://v3." + serverAddress;
                }
            }
        }

        public static string sourceId
        {
            get
            {
                switch (versionNumber)
                {
                    case 1:
                        return "";
                    case 2:
                        return "-090718";
                    case 3:
                    default:
                        return "-111818";
                }
            }
        }

        public const string usWestServer = "us-west";
        public const string usEastServer = "us-east";
        public const string ukServer = "eu-london";
        public const string irelandServer = "eu-ireland";
        public const string germanyServer = "eu-frankfurt";
        public const string canadaServer = "canada";
        public const string japanServer = "ap-tokyo";
        public const string australiaServer = "ap-sydney";
        public const string southKoreaServer = "ap-seoul";
        public const string singaporeServer = "ap-singapore";
        public const string indiaServer = "ap-mumbai";
        public const string brazilServer = "sa";

        public const string version1String = "v1";
        public const string version2String = "v2";
        public const string version3String = "v3";

        public static string endPoint(string prefix)
        {
            return prefix + sourceId + "." + serverAddress;
        }

        public static string apiGetKey
        {
            get
            {
                if (versionNumber < 3)
                {
                    return apiAddress + "/get-key?platform=windows&source=both";
                }
                else
                {
                    return apiAddress + "/get-key?platform=windows";
                }
            }
        }

        public static string apiAccount
        {
            get
            {
                return apiAddress + "/account";
            }
        }

        public static string apiNewSubscription
        {
            get
            {
                return apiAddress + "/new-subscription?plan=all-monthly&locale=";
            }
        }

        public static string apiCreateUser
        {
            get
            {
                if (versionNumber == 1)
                {
                    return apiAddress + "/create-user";
                }
                else
                {
                    return apiAddress + "/signup";
                }
            }
        }

        public static string apiForgotPassword
        {
            get
            {
                return apiAddress + "/forgotPassword";
            }
        }

        public static string apiNewSubscriptionInternal
        {
            get
            {
                return apiAddress + "/newsubscriptioninternal";
            }
        }

        public static string apiSignIn
        {
            get
            {
                return apiAddress + "/signin";
            }
        }

        public const string VPNName = "Confirmed VPN";

        public const string SpeedTestSite = "http://fast.com/";
    }
}
