using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Management;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace TrustWindowsWPF
{
    public class VPNControl
    {
        bool connectedToVPN = false;
        bool running = true;
        bool verifiedConnection = false;
        bool disconnecting = false;
        bool threadRunning = false;
        bool isVirtualMachine = false;

        public VPNControl()
        {
            getConnectionStatus();

            startConnectionCheckThread();

            isVirtualMachine = DetectVM();
        }

        private void startConnectionCheckThread()
        {
            if (!threadRunning)
            {
                threadRunning = true;
                // thread to check connection
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    do
                    {
                        Thread.Sleep(1000);
                        getConnectionStatus();
                    } while (running);

                    threadRunning = false;

                }).Start();
            }
        }

        public bool isConnected()
        {
            return connectedToVPN;
        }

        public bool isRunning()
        {
            return running;
        }

        private bool getConnectionStatus()
        {
            connectedToVPN = RasFuncs.checkConnectedToVPN(ConfirmedConstants.VPNName);

            if(connectedToVPN)
            {
                if (disconnecting)
                {
                    connectedToVPN = false;
                }
                else
                {
                    verifiedConnection = true;
                }
            }
            else
            {
                if(disconnecting)
                {
                    disconnecting = false; // finished disconnecting
                }
            }

            return connectedToVPN;
        }

        public bool toggleVPN(bool turnOn)
        {
            if (turnOn)
            {
                connectToVPN();
            }
            else
            {
                disconnectFromVPN();
            }

            return connectedToVPN;
        }

        public void connectToVPN()
        {
            while(disconnecting) // wait until disconnect done
            {
                Thread.Sleep(100);
            }
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "rasdial.exe";

            pProcess.StartInfo.Arguments = "\"" + ConfirmedConstants.VPNName + "\"";

            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.Start();

            pProcess.WaitForExit();

            //RasFuncs.connectToVPN(VPNName);

            connectedToVPN = true;
            verifiedConnection = false;
        }

        public void disconnectFromVPN()
        {

            connectedToVPN = false;
            verifiedConnection = false;
            disconnecting = true;

            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "rasdial.exe";

            pProcess.StartInfo.Arguments = "\"" + ConfirmedConstants.VPNName + "\" /disconnect";

            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.Start();

            pProcess.WaitForExit();

            //RasFuncs.disconnectVPN(VPNName);
        }

        public bool isVerified()
        {
            return verifiedConnection;
        }

        public void switchServer(string region)
        {
            string newServer = getRegionServerAddress(region);

            if (newServer != "")
            {
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    PowerShellInstance.AddScript("Set-VpnConnection -name \"" + ConfirmedConstants.VPNName + "\" -ServerAddress \"" + newServer + "\"");
                    PowerShellInstance.Invoke();
                }
            }
        }

        static private string getRegionServerAddress(string region)
        {
            string newRegionAddress = "";

            switch (region)
            {
                case "United States - West":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.usWestServer);
                    break;
                case "United States - East":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.usEastServer);
                    break;
                case "United Kingdom":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.ukServer);
                    break;
                case "Ireland":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.irelandServer);
                    break;
                case "Germany":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.germanyServer);
                    break;
                case "Canada":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.canadaServer);
                    break;
                case "Japan":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.japanServer);
                    break;
                case "Australia":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.australiaServer);
                    break;
                case "South Korea":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.southKoreaServer);
                    break;
                case "Singapore":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.singaporeServer);
                    break;
                case "India":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.indiaServer);
                    break;
                case "Brazil":
                    newRegionAddress = ConfirmedConstants.endPoint(ConfirmedConstants.brazilServer);
                    break;
                default:
                    break;
            }

            return newRegionAddress;
        }

        public void addVPN(string region)
        {
            string server = getRegionServerAddress(region);

            if (server != "")
            {
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    PowerShellInstance.AddScript("Add-VpnConnection -name \"" + ConfirmedConstants.VPNName + "\" -ServerAddress \"" + server + "\" -TunnelType IKEv2 -AuthenticationMethod MachineCertificate -EncryptionLevel Required; Set-VpnConnectionIPsecConfiguration -ConnectionName \"" + ConfirmedConstants.VPNName + "\" -AuthenticationTransformConstants GCMAES128 -CipherTransformConstants GCMAES128 -EncryptionMethod AES128 -IntegrityCheckMethod SHA384 -DHGroup ECP256 -PfsGroup ECP256 -Force");
                    PowerShellInstance.Invoke();

                    running = true;

                    startConnectionCheckThread();
                }
            }
            else
            {
                
            }
        }

        public void removeVPN()
        {
            running = false;

            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript("Remove-VpnConnection -name \"" + ConfirmedConstants.VPNName + "\" -Force");
                PowerShellInstance.Invoke();
            }
        }

        static public bool checkCertificateInstalled()
        {
            System.Security.Cryptography.X509Certificates.X509Store store = new System.Security.Cryptography.X509Certificates.X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByIssuerName, ConfirmedConstants.endPoint("www"), true);
            if (certs.Count > 0)
            {
                for(int i=0;i<certs.Count;i++)
                {
                    if (certs[i].IssuerName.Name.ToString() == "CN=" + ConfirmedConstants.endPoint("www"))
                    {
                        return true;
                    }
                }
                
            }
            return false;
        }

        public bool checkVPNExists()
        {
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript("Get-VpnConnection \"Confirmed VPN\"");

                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                if (PSOutput.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool addCertificate(string certPath)
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                if (ConfirmedConstants.versionNumber == 1)
                {
                    startInfo.Arguments = "/C certutil -f -p trustwizardsjustplaying -importpfx \"" + certPath + "\"";
                }
                else
                {
                    startInfo.Arguments = "/C certutil -f -p \"\" -importpfx \"" + certPath + "\"";
                }
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch
            {
                // user didn't authorize
            }

            File.Delete(certPath);

            return true;
        }

        // needs admin
        static public bool addCertificateNeedAdmin(string certPath)
        {
            //X509Certificate2 cert = new X509Certificate2(certPath, "trustwizardsjustplaying", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            X509Certificate2Collection collection = new X509Certificate2Collection();
            collection.Import(certPath, "trustwizardsjustplaying", X509KeyStorageFlags.PersistKeySet);

            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite); //tried MaxAllowed as well

            foreach (X509Certificate2 cert in collection)
            {

                // import to the store

                store.Add(cert);
            }
            store.Close();

            return true;
        }

        public static NetworkInterface GetActiveEthernetOrWifiNetworkInterface()
        {
            var Nic = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(
                a => a.OperationalStatus == OperationalStatus.Up &&
                (a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet) &&
                a.GetIPProperties().GatewayAddresses.Any(g => g.Address.AddressFamily.ToString() == "InterNetwork"));

            return Nic;
        }

        private bool DetectVM()
        {
            using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (var items = searcher.Get())
                {
                    foreach (var item in items)
                    {
                        string manufacturer = item["Manufacturer"].ToString().ToLower();
                        if ((manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                            || manufacturer.Contains("vmware")
                            || item["Model"].ToString() == "VirtualBox")
                        {
                            return true;
                        }
                    }
                }
            }
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        bool dnsAltered = false;
        // make sure interface set to static DNS to avoid DNS leaks
        public void SetDNS()
        {
            if (isVirtualMachine)
            {
                string[] Dns = { "1.1.1.1" };
                var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
                if (CurrentInterface == null) return;

                ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection objMOC = objMC.GetInstances();
                foreach (ManagementObject objMO in objMOC)
                {
                    if ((bool)objMO["IPEnabled"])
                    {
                        if (objMO["Caption"].ToString().Contains(CurrentInterface.Description) && isDNSAuto(CurrentInterface.Id))
                        {
                            ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                            if (objdns != null)
                            {
                                objdns["DNSServerSearchOrder"] = Dns;
                                objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                                dnsAltered = true;
                            }
                        }
                    }
                }
            }
        }

        private bool isDNSAuto(string NetworkAdapterGUID)
        {
            string path = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + NetworkAdapterGUID;
            string ns = (string)Registry.GetValue(path, "NameServer", null);
            if (String.IsNullOrEmpty(ns))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UnsetDNS()
        {
            if (isVirtualMachine && dnsAltered)
            {
                var CurrentInterface = GetActiveEthernetOrWifiNetworkInterface();
                if (CurrentInterface == null) return;

                ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection objMOC = objMC.GetInstances();
                foreach (ManagementObject objMO in objMOC)
                {
                    if ((bool)objMO["IPEnabled"])
                    {
                        if (objMO["Caption"].ToString().Contains(CurrentInterface.Description))
                        {
                            ManagementBaseObject objdns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
                            if (objdns != null)
                            {
                                objdns["DNSServerSearchOrder"] = null;
                                objMO.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                                dnsAltered = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
