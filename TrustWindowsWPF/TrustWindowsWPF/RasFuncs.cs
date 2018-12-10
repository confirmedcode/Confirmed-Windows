using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TrustWindowsWPF
{
    public static class RasFuncs
    {
        [DllImport("rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int RasEnumConnections(
            [In, Out] RASCONN[] rasconn,
            [In, Out] ref int cb,
            [Out] out int connections);

        [DllImport("Rasdlg.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool RasDialDlg(
            IntPtr phoneBook,
            string entryName,
            IntPtr phoneNumber,
            ref RASDIALDLG info);

        [DllImport("rasapi32.dll", SetLastError = true)]
        static extern uint RasGetEntryDialParams(
            string lpszPhonebook,
            [In, Out] ref RASDIALPARAMS lprasdialparams,
            out bool lpfPassword);

        [DllImport("rasapi32.dll", SetLastError = true)]
        public static extern uint RasGetEntryDialParamsW(
        string lpszPhonebook,
        IntPtr lprasdialparams,
        out bool lpfPassword);

        [DllImport("rasapi32.dll", CharSet = CharSet.Unicode)]
        /*static extern uint RasDial(
            uint lpRasDialExtensions,
            string lpszPhonebook,
            //ref RASDIALPARAMS lpRasDialParams,
            IntPtr lpRasDialParams,
            uint dwNotifierType,
            uint lpvNotifier,
            ref IntPtr lphRasConn);
        //ref RASCONN lphRasConn);*/
        private static extern int RasDial(
            IntPtr lpRasDialExtensions,
            string lpszPhonebook,
            IntPtr lpRasDialParams,
            uint dwNotifierType,
            Delegate lpvNotifier,
            ref IntPtr lphRasConn);

        [DllImport("rasapi32.dll", SetLastError = true)]
        static extern uint RasHangUp(IntPtr hRasConn);

        [DllImport("rasapi32.dll", CharSet = CharSet.Auto)]
        private static extern UInt32 RasGetEntryProperties(
            string lpszPhoneBook,
            string szEntry,
            IntPtr lpbEntry,
            ref UInt32 lpdwEntrySize,
            IntPtr lpb,
            IntPtr lpdwSize);

        [DllImport("rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint RasSetEntryProperties(
            string lpszPhonebook,
            string lpszEntry,
            ref RASENTRY lpRasEntry,
            int dwEntryInfoSize,
            IntPtr lpbDeviceInfo,
            int dwDeviceInfoSize);

        const int RAS_MaxEntryName = 256;
        const int RAS_MaxDeviceType = 16;
        const int RAS_MaxDeviceName = 128;
        const int MAX_PATH = 260;
        const int ERROR_BUFFER_TOO_SMALL = 603;
        const int ERROR_SUCCESS = 0;
        const int RAS_MaxAreaCode = 10;
        const int RAS_MaxCallbackNumber = RAS_MaxPhoneNumber;
        const int RAS_MaxDnsSuffix = 256;
        const int RAS_MaxFacilities = 200;
        const int RAS_MaxPadType = 32;
        const int RAS_MaxPath = 260;
        const int RAS_MaxPhoneNumber = 128;
        const int RAS_MaxUserData = 200;
        const int RAS_MaxX25Address = 200;
        const int UsernameLength = 256;
        const int PasswordLength = 256;
        const int DomainLength = 15;


        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        private struct RASCONN
        {
            public int dwSize;
            public IntPtr hrasconn;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName)]
            public string szEntryName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceType)]
            public string szDeviceType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceName)]
            public string szDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szPhonebook;
            public int dwSubEntry;
            public Guid guidEntry;
            public int dwFlags;
            public Guid luid;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        private struct RASDIALDLG
        {
            public int dwSize;
            public IntPtr hwndOwner;
            public int dwFlags;
            public int xDlg;
            public int yDlg;
            public int dwSubEntry;
            public int dwError;
            public IntPtr reserved;
            public IntPtr reserved2;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct RASDIALPARAMS
        {
            public int dwSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName + 1)]
            public string szEntryName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxPhoneNumber + 1)]
            public string szPhoneNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxCallbackNumber + 1)]
            public string szCallbackNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = UsernameLength + 1)]
            public string szUserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PasswordLength + 1)]
            public string szPassword;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = DomainLength + 1)]
            public string szDomain;
            public uint dwSubEntry;
            public IntPtr dwCallbackId;
            //public uint dwIfIndex;
        }

        /*[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct RASDIALPARAMS
        {
            public int dwSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 + 1)]
            public string szEntryName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128 + 1)]
            public string szPhoneNumber;
            [MarshalAs(UnmanagedType.ByValTStr,
            SizeConst = 128 + 1)]
            public string szCallbackNumber;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 + 1)]
            public string szUserName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256 + 1)]
            public string szPassword;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15 + 1)]
            public string szDomain;
            public int dwSubEntrym;
            public int dwCallbackId;
        }*/

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
        struct RASENTRY
        {
            public int dwSize;
            public int dwfOptions;
            public int dwCountryID;
            public int dwCountryCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxAreaCode + 1)]
            public string szAreaCode;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxPhoneNumber + 1)]
            public string szLocalPhoneNumber;
            public int dwAlternateOffset;
            public RASIPADDR ipaddr;
            public RASIPADDR ipaddrDns;
            public RASIPADDR ipaddrDnsAlt;
            public RASIPADDR ipaddrWins;
            public RASIPADDR ipaddrWinsAlt;
            public int dwFrameSize;
            public int dwfNetProtocols;
            public int dwFramingProtocol;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)MAX_PATH)]
            public string szScript;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)MAX_PATH)]
            public string szAutodialDll;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)MAX_PATH)]
            public string szAutodialFunc;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxDeviceType + 1)]
            public string szDeviceType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxDeviceName + 1)]
            public string szDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxPadType + 1)]
            public string szX25PadType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxX25Address + 1)]
            public string szX25Address;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxFacilities + 1)]
            public string szX25Facilities;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxUserData + 1)]
            public string szX25UserData;
            public int dwChannels;
            public int dwReserved1;
            public int dwReserved2;
            public int dwSubEntries;
            public int dwDialMode;
            public int dwDialExtraPercent;
            public int dwDialExtraSampleSeconds;
            public int dwHangUpExtraPercent;
            public int dwHangUpExtraSampleSeconds;
            public int dwIdleDisconnectSeconds;
            public int dwType;
            public int dwEncryptionType;
            public int dwCustomAuthKey;
            public Guid guidId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)MAX_PATH)]
            public string szCustomDialDll;
            public int dwVpnStrategy;
            public int dwfOptions2;
            public int dwfOptions3;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxDnsSuffix)]
            public string szDnsSuffix;
            public int dwTcpWindowSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)MAX_PATH)]
            public string szPrerequisitePbk;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst =
              (int)RAS_MaxEntryName)]
            public string szPrerequisiteEntry;
            public int dwRedialCount;
            public int dwRedialPause;
            RASIPV6ADDR ipv6addrDns;
            RASIPV6ADDR ipv6addrDnsAlt;
            public int dwIPv4InterfaceMetric;
            public int dwIPv6InterfaceMetric;
            RASIPV6ADDR ipv6addr;
            public int dwIPv6PrefixLength;
            public int dwNetworkOutageTime;
        }

        struct RASIPADDR
        {
            byte a;
            byte b;
            byte c;
            byte d;
        }

        struct RASIPV6ADDR
        {
            byte a;
            byte b;
            byte c;
            byte d;
            byte e;
            byte f;
        }

        enum RasEntryOptions
        {
            RASEO_UseCountrAndAreaCodes = 0x00000001,
            RASEO_SpecificIpAddr = 0x00000002,
            RASEO_SpecificNameServers = 0x00000004,
            RASEO_IpHeaderCompression = 0x00000008,
            RASEO_RemoteDefaultGateway = 0x00000010,
            RASEO_DisableLcpExtensions = 0x00000020,
            RASEO_TerminalBeforeDial = 0x00000040,
            RASEO_TerminalAfterDial = 0x00000080,
            RASEO_ModemLights = 0x00000100,
            RASEO_SwCompression = 0x00000200,
            RASEO_RequireEncrptedPw = 0x00000400,
            RASEO_RequireMsEncrptedPw = 0x00000800,
            RASEO_RequireDataEncrption = 0x00001000,
            RASEO_NetworkLogon = 0x00002000,
            RASEO_UseLogonCredentials = 0x00004000,
            RASEO_PromoteAlternates = 0x00008000,
            RASEO_SecureLocalFiles = 0x00010000,
            RASEO_RequireEAP = 0x00020000,
            RASEO_RequirePAP = 0x00040000,
            RASEO_RequireSPAP = 0x00080000,
            RASEO_Custom = 0x00100000,
            RASEO_PreviewPhoneNumber = 0x00200000,
            RASEO_SharedPhoneNumbers = 0x00800000,
            RASEO_PreviewUserPw = 0x01000000,
            RASEO_PreviewDomain = 0x02000000,
            RASEO_ShowDialingProgress = 0x04000000,
            RASEO_RequireCHAP = 0x08000000,
            RASEO_RequireMsCHAP = 0x10000000,
            RASEO_RequireMsCHAP2 = 0x20000000,
            RASEO_RequireW95MSCHAP = 0x40000000

        }

        enum RasEntryOptions2
        {
            None = 0x0,
            SecureFileAndPrint = 0x1,
            SecureClientForMSNet = 0x2,
            DoNotNegotiateMultilink = 0x4,
            DoNotUseRasCredentials = 0x8,
            UsePreSharedKey = 0x10,
            Internet = 0x20,
            DisableNbtOverIP = 0x40,
            UseGlobalDeviceSettings = 0x80,
            ReconnectIfDropped = 0x100,
            SharePhoneNumbers = 0x200,
            SecureRoutingCompartment = 0x400,
            UseTypicalSettings = 0x800,
            IPv6SpecificNameServer = 0x1000,
            IPv6RemoteDefaultGateway = 0x2000,
            RegisterIPWithDns = 0x4000,
            UseDnsSuffixForRegistration = 0x8000,
            IPv4ExplicitMetric = 0x10000,
            IPv6ExplicitMetric = 0x20000,
            DisableIkeNameEkuCheck = 0x40000,
            DisableClassBasedStaticRoute = 0x80000,
            IPv6SpecificAddress = 0x100000,
            DisableMobility = 0x200000,
            RequireMachineCertificates = 0x400000

        }

        enum RasEntryTypes
        {
            RASET_Phone = 1,
            RASET_Vpn = 2,
            RASET_Direct = 3,
            RASET_Internet = 4
        }

        enum RasEntryEncryption
        {
            ET_None = 0,
            ET_Require = 1,
            ET_RequireMax = 2,
            ET_Optional = 3
        }

        enum RasNetProtocols
        {
            RASNP_NetBEUI = 0x00000001,
            RASNP_Ipx = 0x00000002,
            RASNP_Ip = 0x00000004,
            RASNP_Ipv6 = 0x00000008
        }

        enum RasFramingProtocol
        {
            RASFP_Ppp = 0x00000001,
            RASFP_Slip = 0x00000002,
            RASFP_Ras = 0x00000004
        }


        public static bool checkConnectedToVPN(string vpnName)
        {
            int cb = 0, connectionCount;
            if (RasEnumConnections(null, ref cb, out connectionCount) == ERROR_BUFFER_TOO_SMALL)
            {
                if (connectionCount == 0) return false;
                RASCONN[] buffer = new RASCONN[connectionCount];
                buffer[0].dwSize = Marshal.SizeOf(typeof(RASCONN));


                if (RasEnumConnections(buffer, ref cb, out connectionCount) == ERROR_SUCCESS)
                {
                    foreach (RASCONN rasConn in buffer)
                    {
                        if (rasConn.szEntryName == vpnName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool connectToVPNDlg(string vpnName)
        {
            RASDIALDLG info = new RASDIALDLG();
            info.dwSize = Marshal.SizeOf(info);
            bool ret = RasDialDlg(IntPtr.Zero, vpnName, IntPtr.Zero, ref info);

            return ret;
        }

        public static bool connectToVPN(string vpnName)
        {
            // Initialize unmanged memory to hold the struct.
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RASDIALPARAMS)));

            var lpRasDialParams = new RASDIALPARAMS();
            lpRasDialParams.szEntryName = vpnName;
            lpRasDialParams.dwSize = Marshal.SizeOf(typeof(RASDIALPARAMS));

            // Copy the struct to unmanaged memory.
            Marshal.StructureToPtr(lpRasDialParams, pnt, true);

            bool lprPassword = false;

            var nRet = RasGetEntryDialParamsW(null, pnt, out lprPassword);
            if (nRet != 0)
            {
                // Clear unmanaged memory
                Marshal.FreeHGlobal(pnt);
                //throw new Exception("Error text");
                return false;
            }

            // Copy unmanaged memory to the struct.
            //lpRasDialParams = (RASDIALPARAMS)Marshal.PtrToStructure(pnt, typeof(RASDIALPARAMS));

            // Clear unmanaged memory
            //Marshal.FreeHGlobal(pnt);

            //RASCONN ras = new RASCONN();
            //ras.dwSize = Marshal.SizeOf(typeof(RASCONN));
            IntPtr hrassconn = IntPtr.Zero; //Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RASCONN)));
            //RasHandle rhandle = null;

            var ret = RasDial(IntPtr.Zero, null, pnt, 0, null, ref hrassconn);

            return true;
        }

        public static bool disconnectVPN(string vpnName)
        {
            int cb = 0, connectionCount;
            if (RasEnumConnections(null, ref cb, out connectionCount) == ERROR_BUFFER_TOO_SMALL)
            {
                if (connectionCount == 0) return false;
                RASCONN[] buffer = new RASCONN[connectionCount];
                buffer[0].dwSize = Marshal.SizeOf(typeof(RASCONN));


                if (RasEnumConnections(buffer, ref cb, out connectionCount) == ERROR_SUCCESS)
                {
                    foreach (RASCONN rasConn in buffer)
                    {
                        if (rasConn.szEntryName == vpnName)
                        {
                            if (RasHangUp(rasConn.hrasconn) == 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool createIKEV2VPN(string name, string server)
        {
            UInt32 dwSize = 0;
            RasGetEntryProperties(null, "", IntPtr.Zero, ref dwSize, IntPtr.Zero, IntPtr.Zero);

            RASENTRY rasE = new RASENTRY();
            rasE.dwSize = (int)dwSize;
            rasE.dwType = (int)RasEntryTypes.RASET_Vpn;
            rasE.dwRedialCount = 1;
            rasE.dwRedialPause = 60;
            rasE.dwfNetProtocols = (int)RasNetProtocols.RASNP_Ip;
            rasE.dwEncryptionType = (int)RasEntryEncryption.ET_Optional;
            rasE.szLocalPhoneNumber = server;
            rasE.szDeviceType = "RASDT_Vpn";
            rasE.dwfOptions = (int)RasEntryOptions.RASEO_RemoteDefaultGateway | (int)RasEntryOptions.RASEO_RequireDataEncrption;
            rasE.dwfOptions2 = (int)RasEntryOptions2.RequireMachineCertificates;
            rasE.dwVpnStrategy = 7; // IKEV2?

            uint ret = RasSetEntryProperties(null, name, ref rasE, rasE.dwSize, IntPtr.Zero, 0);

            return true;
        }
    }
}
