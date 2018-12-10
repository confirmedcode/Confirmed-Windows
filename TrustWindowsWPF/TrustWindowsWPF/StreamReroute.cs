using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.InteropServices;

namespace TrustWindowsWPF
{
    public class StreamReroute
    {
        const string AWSRangesFile = "AWS Ranges.txt";
        const string NetflixRangesFile = "Netflix Ranges.txt";

        List<IPAddressRange> awsRanges;
        List<IPAddressRange> netflixRanges;
        IPAddress gateway;
        int gatewayIndex;

        List<RouteItem> changedRoutes;

        bool doRerouting = false;
        bool isRerouting = false;

        private static object _LOCK = new object();

        private class RouteItem
        {
            public string URL;
            public List<IPAddress> ipAddresses;

            public RouteItem(string url)
            {
                URL = url;
                ipAddresses = new List<IPAddress>();
            }

            public bool AddIpAddress(IPAddress newIP)
            {
                bool found = false;
                foreach(IPAddress ip in ipAddresses)
                {
                    if(ip.Equals(newIP))
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    ipAddresses.Add(newIP);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public StreamReroute()
        {
            
            awsRanges = new List<IPAddressRange>();
            netflixRanges = new List<IPAddressRange>();
            //LoadAWSRanges();
            LoadNetflixRanges();
            changedRoutes = new List<RouteItem>();
        }

        bool getGateWay()
        {
            gateway = null; 
            NetworkInterface gatewayData = GetDefaultGateway();

            if (gatewayData != null)
            {
                IPInterfaceProperties gateProps = gatewayData.GetIPProperties();

                foreach(var gateAd in gateProps.GatewayAddresses)
                {
                    if(!gateAd.Address.IsIPv6LinkLocal)
                    {
                        gateway = gateAd.Address;
                        break;
                    }
                }
                gatewayIndex = gateProps.GetIPv4Properties().Index;
                return true;
            }
            else
            {
                return false;
            }
        }

        void LoadAWSRanges()
        {
            double ipTotal = 0;

            using (StreamReader r = new StreamReader(AWSRangesFile))
            {
                string jsonString = r.ReadToEnd();
                //List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);
                foreach (var prefix in obj["prefixes"])
                {
                    awsRanges.Add(new IPAddressRange(prefix["ip_prefix"].Value));

                    string ipTest = prefix["ip_prefix"].Value;
                    var ipSplit = ipTest.Trim().Split('/');
                    ipTotal += Math.Pow(2, 32 - Int32.Parse(ipSplit[1]));
                }
            }

            int i = 0;
        }

        void LoadNetflixRanges()
        {
            try
            {
                using (StreamReader r = new StreamReader(System.Windows.Forms.Application.StartupPath + "\\" + NetflixRangesFile))
                {
                    string netflixString = r.ReadToEnd();

                    var result = netflixString.Split(new[] { '\r', '\n' });

                    foreach (string line in result)
                    {
                        string trimmedLine = line.Trim();
                        if (trimmedLine.Length > 0)
                        {
                            netflixRanges.Add(new IPAddressRange(trimmedLine));
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }
        }

        public void StartRerouting(VPNControl vpnControl)
        {
            if (!isRerouting && vpnControl.isConnected())
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    if (getGateWay())
                    {
                        isRerouting = true;
                        StringCollection urls = Properties.Settings.Default.rerouteURLs;

                        lock (_LOCK)
                        {
                            foreach (string url in urls)
                            {
                                addRerouteItem(url);
                            }
                        }

                        if (Properties.Settings.Default.rerouteNetflix)
                        {
                            RerouteNetflix();
                        }

                        if (Properties.Settings.Default.rerouteHulu)
                        {
                            RerouteHulu();
                        }

                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            do
                            {
                                Thread.Sleep(500);

                                CheckDNSCache();

                            } while (Properties.Settings.Default.streamReroute && vpnControl.isConnected());

                            isRerouting = false;
                            removeAllReroute();
                        }).Start();
                    }
                }).Start();
            }
        }

        public void RerouteHulu()
        {
            if (isRerouting)
            {
                /*System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C nslookup player.hulu.com";
                startInfo.CreateNoWindow = true;
                //startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var result = output.Split(new[] { '\r', '\n' });
                bool sawFirst = false;
                string theAddress = null;

                foreach (string line in result)
                {
                    if (line.Length > 7)
                    {
                        if (line.Substring(0, 7) == "Address")
                        {
                            // second address is what we want
                            if (sawFirst)
                            {
                                var addressSplit = line.Trim().Split(':');
                                theAddress = addressSplit[1].Trim();
                                break;
                            }
                            else
                            {
                                sawFirst = true;
                            }
                        }
                    }
                }

                if (theAddress != null)
                {
                    /*process = new System.Diagnostics.Process();
                    startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C route add " + theAddress + " " + gateway;
                    startInfo.Verb = "runas";
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();*/
                /*RouteInterop.AddRoute(theAddress, "255.255.255.255", gateway.ToString(), gatewayIndex);
            }*/

                lock (_LOCK)
                {
                    addRerouteItem("hulu.com");
                }

                // need to get this subdomain specifically
                DnsFuncs.GetDnsRecords("player.hulu.com", false);
            }
        }

        public void RerouteNetflix()
        {
            if (isRerouting)
            {
                foreach (IPAddressRange range in netflixRanges)
                {
                    RouteInterop.AddRoute(range.ipAddressLower, range.mask, gateway, gatewayIndex);
                }

                lock (_LOCK)
                {
                    addRerouteItem("netflix.com");
                    addRerouteItem("nflxext.com");
                }

                // Go ahead and ping these urls so they go in cache
                DnsFuncs.GetDnsRecords("secure.netflix.com", false);
                DnsFuncs.GetDnsRecords("codex.nflxext.com", false);
                DnsFuncs.GetDnsRecords("assets.nflxext.com", false);
                DnsFuncs.GetDnsRecords("api-global.netflix.com", false);
                DnsFuncs.GetDnsRecords("customerevents.netflix.com", false);
            }
        }

        public void addRerouteItem(string url)
        {
            if (isRerouting)
            {
                // check no duplicate
                bool found = false;
                lock (_LOCK)
                {
                    foreach (RouteItem rItem in changedRoutes)
                    {
                        if (rItem.URL == url)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        RouteItem newItem = new RouteItem(url);
                        changedRoutes.Add(newItem);

                        List<String> results = DnsFuncs.GetDnsRecords(url, false);
                        foreach (string ipAddress in results)
                        {
                            if (newItem.AddIpAddress(IPAddress.Parse(ipAddress)))
                            {
                                RouteInterop.AddRoute(ipAddress.Trim(), "255.255.255.255", gateway.ToString(), gatewayIndex);
                            }
                        }
                    }
                }
            }
        }

        public void removeRerouteItem(string url)
        {
            if (isRerouting)
            {
                RouteItem toRemove = null;

                lock (_LOCK)
                {
                    foreach (RouteItem rItem in changedRoutes)
                    {
                        if (rItem.URL == url)
                        {
                            toRemove = rItem;
                            break;
                        }
                    }

                    if (toRemove != null)
                    {
                        foreach (IPAddress ip in toRemove.ipAddresses)
                        {
                            RouteInterop.DeleteRoute(ip, IPAddress.Parse("255.255.255.255"), gateway, gatewayIndex);
                        }
                    }

                    changedRoutes.Remove(toRemove);
                }
            }
        }

        private void removeAllReroute()
        {
            lock(_LOCK)
            {
                foreach(RouteItem rItem in changedRoutes)
                {
                    foreach (IPAddress ip in rItem.ipAddresses)
                    {
                        RouteInterop.DeleteRoute(ip, IPAddress.Parse("255.255.255.255"), gateway, gatewayIndex);
                    }
                    rItem.ipAddresses.Clear();
                }


            }

            if (Properties.Settings.Default.rerouteNetflix)
            {
                RemoveNetflixRanges();
            }
        }

        public void endNetflixReroute()
        {
            if (isRerouting)
            {
                removeRerouteItem("netflix.com");
                removeRerouteItem("nflxext.com");

                RemoveNetflixRanges();
            }
        }

        public void endHuluReroute()
        {
            if (isRerouting)
            {
                removeRerouteItem("hulu.com");
            }
        }

        void RemoveNetflixRanges()
        {
            foreach (IPAddressRange range in netflixRanges)
            {
                RouteInterop.DeleteRoute(range.ipAddressLower, range.mask, gateway, gatewayIndex);
            }
        }

        public void setRerouting(bool reroute)
        {
            doRerouting = reroute;
        }

        void CheckDNSCache()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C ipconfig /displaydns";
            startInfo.CreateNoWindow = true;
            //startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var result = output.Split(new[] { '\r', '\n' });

            bool netflixSpotted = false;
            string cname = null;
            RouteItem currItem = null;
            foreach (string line in result)
            {
                if(line.Length > 20)
                {
                    string trimmedLine = line.Trim();
                    if(trimmedLine.Substring(0,11) == "Record Name")
                    {
                        if (cname != null)
                        {
                            netflixSpotted = trimmedLine.Contains(cname);
                        }
                        else
                        {
                            /*netflixSpotted = trimmedLine.Contains("netflix.com") || trimmedLine.Contains("nflxext.com");

                            StringCollection urls = Properties.Settings.Default.rerouteURLs;*/

                            lock (_LOCK)
                            {
                                foreach (RouteItem rItem in changedRoutes)
                                {
                                    int index = trimmedLine.IndexOf(rItem.URL);
                                    if(index > 0)
                                    {
                                        if(trimmedLine[index-1] == '.' || trimmedLine[index-1] == ' ')
                                        {
                                            netflixSpotted = true;
                                            currItem = rItem;
                                            break;
                                        }
                                    }

                                    /*if (trimmedLine.Contains(rItem.URL))
                                    {
                                        netflixSpotted = true;
                                        currItem = rItem;
                                        break;
                                    }*/
                                }
                            }
                        }
                    }
                    else if(netflixSpotted)
                    {
                        if(trimmedLine.Substring(0,12) == "CNAME Record")
                        {
                            cname = trimmedLine.Split(':')[1].Trim();
                            netflixSpotted = false;
                        }
                        if(trimmedLine.Substring(0,15) == "A (Host) Record")
                        {
                            cname = null;

                            var split = trimmedLine.Split(':');
                            if(split.Count<string>() == 2) // not ipv6
                            {
                                /*process = new System.Diagnostics.Process();
                                startInfo = new System.Diagnostics.ProcessStartInfo();
                                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                startInfo.FileName = "cmd.exe";
                                startInfo.Arguments = "/C route add " + split[1].Trim() + " mask 255.255.255.255 " + gateway;
                                startInfo.Verb = "runas";
                                process.StartInfo = startInfo;
                                process.Start();
                                process.WaitForExit();*/
                                string newIp = split[1].Trim();
                                if(currItem != null)
                                {
                                    // if not added
                                    if (currItem.AddIpAddress(IPAddress.Parse(newIp)))
                                    {
                                        RouteInterop.AddRoute(split[1].Trim(), "255.255.255.255", gateway.ToString(), gatewayIndex);
                                    }
                                    else
                                    {
                                        int i = 0;
                                    }
                                }

                                netflixSpotted = false;
                                currItem = null;
                            }
                        }
                    }
                }
            }

            

            /*var results = DnsFuncs.GetDnsTable();

            foreach (string ipAddress in results)
            {
                RouteInterop.AddRoute(ipAddress.Trim(), "255.255.255.255", gateway.ToString(), gatewayIndex);
            }

            AddRerouteByDNSName("netflix.com");
            AddRerouteByDNSName("www.netflix.com");
            AddRerouteByDNSName("secure.netflix.com");
            AddRerouteByDNSName("codex.nflxext.com");
            AddRerouteByDNSName("assets.nflxext.com");
            AddRerouteByDNSName("api-global.netflix.com");
            AddRerouteByDNSName("customerevents.netflix.com");*/
        }

        void AddRerouteByDNSName(string name)
        {
            var results = DnsFuncs.GetDnsRecords(name);

            foreach (string ipAddress in results)
            {
                /*System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C route add " + ipAddress.Trim() + " mask 255.255.255.255 " + gateway;
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();*/
                RouteInterop.AddRoute(ipAddress.Trim(), "255.255.255.255", gateway.ToString(), gatewayIndex);
            }
        }

        public static NetworkInterface GetDefaultGateway()
        {
            /*var gateways = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .Where(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0);*/
            //.FirstOrDefault();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Where(n => n.GetIPProperties()?.GatewayAddresses.Count != 0)
                .Where(n => n.Description != "Confirmed VPN");

            //return gateways.FirstOrDefault();
            return interfaces.FirstOrDefault();
        }

        void FindRangeAndAdd(string ipAddress)
        {
            bool found = false;
            foreach (IPAddressRange range in awsRanges)
            {
                if (range.IsInRange(ipAddress))
                {
                    found = true;
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = "/C route add " + range.ipAddressLower + " mask " + range.mask + " " + gateway;
                    startInfo.Verb = "runas";
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();

                    break;
                }
            }

            if (!found)
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C route add " + ipAddress + " " + gateway;
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
        }
    }

    public class IPAddressRange
    {
        public IPAddress ipAddressLower;
        public IPAddress mask;

        public IPAddressRange(string ipAdressRangeString)
        {
            string[] rangeSplit = ipAdressRangeString.Split('/');
            ipAddressLower = IPAddress.Parse(rangeSplit[0]);
            int bits = Int32.Parse(rangeSplit[1]);

            byte[] maskBytes = { 0, 0, 0, 0 };
            int byteCount = 0;
            while (byteCount < 4)
            {
                if (bits >= 8)
                {
                    maskBytes[byteCount] = 0xFF;
                    bits -= 8;
                }
                else if (bits > 0)
                {
                    byte bitMask = 0x80;
                    for (int i = 0; i < bits; i++)
                    {
                        maskBytes[byteCount] |= bitMask;
                        bitMask = (byte)(bitMask >> 1);
                    }

                    bits = 0;
                }
                byteCount++;
            }

            mask = new IPAddress(maskBytes);
        }

        public bool IsInRange(string addressString)
        {
            return IsInRange(IPAddress.Parse(addressString));
        }

        public bool IsInRange(IPAddress address)
        {
            byte[] addressBytes = address.GetAddressBytes();
            byte[] maskBytes = mask.GetAddressBytes();
            for (int i = 0; i < 4; i++)
            {
                addressBytes[i] &= maskBytes[i];
            }

            IPAddress compareAddress = new IPAddress(addressBytes);

            return ipAddressLower.Equals(compareAddress);
        }
    }

    public static class DnsFuncs
    {
        [DllImport("dnsapi", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsQuery(
            [MarshalAs(UnmanagedType.VBByRefStr)]ref string pszName,
            QueryTypes wType,
            QueryOptions options,
            int aipServers,
            ref IntPtr ppQueryResults,
            int pReserved);

        [DllImport("dnsapi", EntryPoint = "DnsGetCacheDataTable", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern int DnsGetCacheDataTable(
            ref IntPtr ppQueryResults);

        [DllImport("dnsapi", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void DnsRecordListFree(
            IntPtr pRecordList,
            int FreeType);

        private enum QueryOptions
        {
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
            DNS_QUERY_BYPASS_CACHE = 8,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_NO_RECURSION = 4,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_RESERVED = -16777216,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_STANDARD = 0,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_USE_TCP_ONLY = 2,
            DNS_QUERY_WIRE_ONLY = 0x100
        }

        private enum QueryTypes
        {
            DNS_TYPE_A = 1,
            DNS_TYPE_NS = 2,
            DNS_TYPE_CNAME = 5,
            DNS_TYPE_SOA = 6,
            DNS_TYPE_PTR = 12,
            DNS_TYPE_HINFO = 13,
            DNS_TYPE_MX = 15,
            DNS_TYPE_TXT = 16,
            DNS_TYPE_AAAA = 28
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DNS_RECORD
        {
            public IntPtr pNext;
            public IntPtr pName;
            public short wType;
            public short wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public DnsData DATA;
            public short wPreference;
            public short Pad;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DNS_CACHE_ENTRY
        {
            public IntPtr pNext;
            public IntPtr pName;
            public short wType;
            public short wDataLength;
            public int flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DnsData
        {
            public DNS_A_DATA A;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DNS_A_DATA
        {
            public IP4_ADDRESS IpAddress;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IP4_ADDRESS
        {
            public UInt32 Ip4Addr;
        }

        public static List<string> GetDnsRecords(string domain)
        {
            return GetDnsRecords(domain, true);
        }

        public static List<string> GetDnsRecords(string domain, bool cacheOnly)
        {
            IntPtr ptr1 = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            DNS_RECORD recDns;
            DNS_A_DATA recA;
            List<string> Results = new List<string>();

            QueryOptions qOptions = QueryOptions.DNS_QUERY_NO_HOSTS_FILE;
            if(cacheOnly)
            {
                qOptions |= QueryOptions.DNS_QUERY_NO_WIRE_QUERY;
            }

            int RtnVal = DnsQuery(
                ref domain,
                QueryTypes.DNS_TYPE_A,
                qOptions,
                0,
                ref ptr1,
                0);

            if (RtnVal != 0)
            {
                //throw new Win32Exception(RtnVal);
            }

           

            for (ptr2 = ptr1; !ptr2.Equals(IntPtr.Zero); ptr2 = recDns.pNext)
            {
                recDns = (DNS_RECORD)Marshal.PtrToStructure(ptr2, typeof(DNS_RECORD));

                if (recDns.wType == 1)
                {
                    string name = Marshal.PtrToStringAuto(recDns.pName);

                    recA = (DNS_A_DATA)recDns.DATA.A;
                    string ip = ReverseIPAddr(recA.IpAddress.Ip4Addr);
                    Results.Add(ip);

                    if (name.Contains("netflix"))
                    {
                        int i = 0;
                    }
                }
            }

            DnsRecordListFree(ptr2, 0);

            return Results;
        }

        public static List<string> GetDnsTable()
        {
            IntPtr ptr1 = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            DNS_CACHE_ENTRY recDns;
            DNS_A_DATA recA;
            List<string> Results = new List<string>();

            int RtnVal = DnsGetCacheDataTable(
                ref ptr1);

            if (RtnVal != 0)
            {
                //throw new Win32Exception(RtnVal);
            }

            for (ptr2 = ptr1; !ptr2.Equals(IntPtr.Zero); ptr2 = recDns.pNext)
            {
                recDns = (DNS_CACHE_ENTRY)Marshal.PtrToStructure(ptr2, typeof(DNS_CACHE_ENTRY));
                if (recDns.wType == 1)
                {
                    string name = Marshal.PtrToStringAuto(recDns.pName);

                    //for (int i = 0; i < domains.Count; i++)
                    // {
                    if (name.Contains("netflix.com") || name.Contains("nflxext.com"))
                    {
                        /*recA = (DNS_A_DATA)recDns.DATA.A;
                        string ip = ReverseIPAddr(recA.IpAddress.Ip4Addr);
                        Results.Add(ip);*/
                        int i = 0;
                    }
                   // }
                }
            }

            DnsRecordListFree(ptr2, 0);

            return Results;
        }

        private static string ReverseIPAddr(UInt32 longIP)
        {
            IPAddress ip = IPAddress.Parse(longIP.ToString());
            byte[] ipBytes = ip.GetAddressBytes();
            Array.Reverse(ipBytes);
            string ipAddress = new IPAddress(ipBytes).ToString();
            return ipAddress;
        }
    }

    public static class RouteInterop
    {
        [DllImport("Iphlpapi.dll")]

        [return: MarshalAs(UnmanagedType.U4)]

        static extern int CreateIpForwardEntry(ref MIB_IPFORWARDROW pRoute);

        [DllImport("Iphlpapi.dll")]

        [return: MarshalAs(UnmanagedType.U4)]

        static extern int DeleteIpForwardEntry(ref MIB_IPFORWARDROW pRoute);

        public enum ForwardType
        {
            Other = 1,
            Invalid = 2,
            Direct = 3,
            Indirect = 4
        }

        public enum ForwardProtocol
        {
            Other = 1,
            Local = 2,
            NetMGMT = 3,
            ICMP = 4,
            EGP = 5,
            GGP = 6,
            Hello = 7,
            RIP = 8,
            IS_IS = 9,
            ES_IS = 10,
            CISCO = 11,
            BBN = 12,
            OSPF = 13,
            BGP = 14,
            NT_AUTOSTATIC = 10002,
            NT_STATIC = 10006,
            NT_STATIC_NON_DOD = 10007
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPFORWARDROW
        {
            public UInt32 dwForwardDest;
            public UInt32 dwForwardMask;
            public int dwForwardPolicy;
            public UInt32 dwForwardNextHop;
            public int dwForwardIfIndex;
            public ForwardType dwForwardType;
            public ForwardProtocol dwForwardProto;
            public int dwForwardAge;
            public int dwForwardNextHopAS;
            public int dwForwardMetric1;
            public int dwForwardMetric2;
            public int dwForwardMetric3;
            public int dwForwardMetric4;
            public int dwForwardMetric5;
        }

        /*[DllImport("Iphlpapi.dll", CharSet = CharSet.Auto)]
        public static extern int GetInterfaceInfo(Byte[] PIfTableBuffer, ref int size);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct IP_ADAPTER_INDEX_MAP
        {
            public int Index;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = PInvokes.MAX_ADAPTER_NAME)]
            public String Name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct IP_INTERFACE_INFO
        {
            public int NumAdapters;
            public IP_ADAPTER_INDEX_MAP[] Adapter;

            public static IP_INTERFACE_INFO FromByteArray(Byte[] buffer)
            {
                unsafe
                {
                    IP_INTERFACE_INFO rv = new IP_INTERFACE_INFO();
                    int iNumAdapters = 0;
                    Marshal.Copy(buffer, 0, new IntPtr(&iNumAdapters), 4);
                    IP_ADAPTER_INDEX_MAP[] adapters = new IP_ADAPTER_INDEX_MAP[iNumAdapters];
                    rv.NumAdapters = iNumAdapters;
                    rv.Adapter = new IP_ADAPTER_INDEX_MAP[iNumAdapters];
                    int offset = sizeof(int);
                    for (int i = 0; i < iNumAdapters; i++)
                    {
                        IP_ADAPTER_INDEX_MAP map = new IP_ADAPTER_INDEX_MAP();
                        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(map));
                        Marshal.StructureToPtr(map, ptr, false);
                        Marshal.Copy(buffer, offset, ptr, Marshal.SizeOf(map));
                        map = (IP_ADAPTER_INDEX_MAP)Marshal.PtrToStructure(ptr, typeof(IP_ADAPTER_INDEX_MAP));
                        Marshal.FreeHGlobal(ptr);
                        rv.Adapter[i] = map;
                        offset += Marshal.SizeOf(map);
                    }
                    return rv;
                }
            }
        }

        public static IP_INTERFACE_INFO GetInterfaceInfo()
        {
            int size = 0;
            int r = GetInterfaceInfo(null, ref size);
            Byte[] buffer = new Byte[size];
            r = GetInterfaceInfo(buffer, ref size);
            if (r != ERROR_SUCCESS)
                throw new Exception("GetInterfaceInfo returned an error.");
            IP_INTERFACE_INFO info = IP_INTERFACE_INFO.FromByteArray(buffer);
            return info;
        }*/

        public static void AddRoute(string ipAddress, string mask, string gateway, int gatewayIndex)
        {
            AddRoute(IPAddress.Parse(ipAddress), IPAddress.Parse(mask), IPAddress.Parse(gateway), gatewayIndex);
        }

        public static void AddRoute(IPAddress ipAddress, IPAddress mask, IPAddress gateway, int gatewayIndex)
        {

            if (gateway != null)
            {
                var route = new MIB_IPFORWARDROW
                {
                    dwForwardDest = BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0),
                    dwForwardMask = BitConverter.ToUInt32(mask.GetAddressBytes(), 0),
                    dwForwardNextHop = BitConverter.ToUInt32(gateway.GetAddressBytes(), 0),
                    dwForwardNextHopAS = 0,
                    dwForwardPolicy = 0,
                    dwForwardMetric1 = 9999,
                    dwForwardType = ForwardType.Indirect,
                    dwForwardProto = ForwardProtocol.NetMGMT,
                    dwForwardAge = 0,
                    dwForwardIfIndex = gatewayIndex,
                    dwForwardMetric2 = -1,
                    dwForwardMetric3 = -1,
                    dwForwardMetric4 = -1,
                    dwForwardMetric5 = -1
                };

                int output = CreateIpForwardEntry(ref route);
            }
        }

        public static void DeleteRoute(IPAddress ipAddress, IPAddress mask, IPAddress gateway, int gatewayIndex)
        {
            if (gateway != null)
            {
                var route = new MIB_IPFORWARDROW
                {
                    dwForwardDest = BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0),
                    dwForwardMask = BitConverter.ToUInt32(mask.GetAddressBytes(), 0),
                    dwForwardNextHop = BitConverter.ToUInt32(gateway.GetAddressBytes(), 0),
                    dwForwardNextHopAS = 0,
                    dwForwardPolicy = 0,
                    dwForwardMetric1 = 9999,
                    dwForwardType = ForwardType.Indirect,
                    dwForwardProto = ForwardProtocol.NetMGMT,
                    dwForwardAge = 0,
                    dwForwardIfIndex = gatewayIndex,
                    dwForwardMetric2 = -1,
                    dwForwardMetric3 = -1,
                    dwForwardMetric4 = -1,
                    dwForwardMetric5 = -1
                };

                int output = DeleteIpForwardEntry(ref route);
            }
        }
    }
}
