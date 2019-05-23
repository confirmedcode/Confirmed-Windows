using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrustWindowsWPF
{
    public class ComboBoxItem
    {
        public ImageSource Image { get; set; }
        public string Label { get; set; }

        public ComboBoxItem(ImageSource image, string label)
        {
            Image = image;
            Label = label;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        ObservableCollection<ComboBoxItem> Regions = new ObservableCollection<ComboBoxItem>();
        VPNControl vpnControl = new VPNControl();
        System.Windows.Threading.DispatcherTimer connectionTimer;
        StreamReroute sReroute;
        VPNSettings vpnSettings = null;

        APIControl apiControl = new APIControl();

        private bool noInternet = false;

        private LoginWindow login = null;

        public bool isConnecting { get; set; }
        public bool wantsConnect { get; set; }

        public MainWindow()
        {
            DataContext = this;

            wantsConnect = false;

            sReroute = new StreamReroute();

            InitializeComponent();

            if (Properties.Settings.Default.needsUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.needsUpgrade = false;
                Properties.Settings.Default.Save();
            }

            this.Topmost = true;

            PopulateComboBox();

            bool needsLoginWindow = true;

            if (checkLogin())
            {
                // VPN for some reason still in place
                if (vpnControl.checkVPNExists())
                {
                    //initTimer();
                    needsLoginWindow = false;

                    setConnecting();
                }
                // else check certificate installed; then go ahead and add back VPN
                else if (VPNControl.checkCertificateInstalled() && !noInternet)
                {
                    needsLoginWindow = false;
                    vpnControl.addVPN(Properties.Settings.Default.region);

                    setConnecting();

                    //initTimer();
                }
                else if (noInternet && !needsLoginWindow)
                {
                    System.Windows.Forms.MessageBox.Show("Can't connect to server.");
                }
                else if (!VPNControl.checkCertificateInstalled() && !noInternet)
                {
                    // reinstall certificates
                    var getKeyResponse = apiControl.getKey();
                    var getKeyResponseString = getKeyResponse.Result.Content.ReadAsStringAsync();

                    var getKeyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(getKeyResponseString.Result);

                    if (getKeyDict.ContainsKey("b64"))
                    {
                        string tempFile = System.IO.Path.GetTempFileName();

                        File.WriteAllBytes(tempFile, Convert.FromBase64String(getKeyDict["b64"]));

                        VPNControl.addCertificate(tempFile);

                        needsLoginWindow = false;
                        vpnControl.addVPN(Properties.Settings.Default.region);

                        setConnecting();
                    }
                }
                else
                {
                    needsLoginWindow = true;
                }
            }

            // if not ready to run VPN, send to login window
            if (needsLoginWindow)
            {
                handleLoginForm(false);
            }
            else
            {
                notifyIcon.Visible = true;
                initTimer();
            }
            //initNotifyIcon();

            // For crash testing
            //int x = 0;
            //int y = 1 / x;

            //SetWindowPosition();
        }

        

        private void handleLoginForm(bool afterStarting)
        {
            login = new LoginWindow(apiControl);
            login.ShowDialog();

            if (checkLogin())
            {
                vpnControl.addVPN(Properties.Settings.Default.region);

                notifyIcon.Visible = true;

                SetComboBoxSelection();

                connectionAttempts = 0;

                // Connect to VPN for first time
                setConnecting();
                initTimer();
                this.Show();
                this.Activate();
            }
            // if login exited without completing, close program
            else
            {
                // different closes works in different situations for some reason
                if (!afterStarting)
                {
                    System.Windows.Forms.Application.Exit();
                    System.Environment.Exit(1);
                }
                else
                {
                    this.Close();
                }
            }
        }

        private bool checkLogin()
        {
            try
            {
                return apiControl.checkStoredLogin();
            }
            catch(Exception e)
            {
                noInternet = true;
            }

            return false;
        }

        private void SetWindowPosition()
        {
            double left, right;
            TaskBarLocationProvider.CalculateWindowPositionByTaskbar(this.ActualWidth, this.ActualHeight, this, 10, out left, out right);

            Left = left;
            Top = right;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            this.Hide();
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public bool checkIsConnecting()
        {
            return isConnecting;
        }

        protected override void OnInitialized(EventArgs e)
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();

            IconHandles = new Dictionary<string, System.Drawing.Icon>();
            IconHandles.Add("QuickLaunch", new System.Drawing.Icon(System.IO.Path.Combine(Environment.CurrentDirectory, @"tunnel_icon_transparent_gfm_icon.ico")));

            notifyIcon.Icon = IconHandles["QuickLaunch"];

            Loaded += OnLoaded;

            notifyIcon.Click += notifyIcon_Click;
            Closed += OnClosed;
            /*notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            notifyIcon.Icon = IconHandles["QuickLaunch"];
            Loaded += OnLoaded;
            StateChanged += OnStateChanged;
            Closing += OnClosing;
            Closed += OnClosed;
            Dispatcher.UnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Dispatcher_UnhandledException);*/

            base.OnInitialized(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            notifyIcon.Visible = true;
            SetWindowPosition();
        }

        private void OnClosed(object sender, System.EventArgs e)
        {
            try
            {
                if (notifyIcon != null)
                {
                    notifyIcon.Visible = false;
                    notifyIcon.Dispose();
                    notifyIcon = null;
                }
            }
            catch (Exception ex)
            {
                //ex.Log();
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            SetWindowPosition();
            this.Show();
            this.Activate();
        }

        private Dictionary<string, System.Drawing.Icon> IconHandles = null;
        private void initNotifyIcon()
        {
            notifyIcon = new System.Windows.Forms.NotifyIcon();

            IconHandles = new Dictionary<string, System.Drawing.Icon>();
            IconHandles.Add("QuickLaunch", new System.Drawing.Icon(System.IO.Path.Combine(Environment.CurrentDirectory, @"tunnel_icon_transparent_gfm_icon.ico")));
            notifyIcon.Visible = true;
        }

        private void initTimer()
        {
            connectionTimer = new System.Windows.Threading.DispatcherTimer();
            connectionTimer.Tick += connectCheckTimer;
            connectionTimer.Interval = new TimeSpan(0, 0, 1);
            connectionTimer.Start();
        }

        int connectionAttempts = 0;
        private void connectCheckTimer(object sender, EventArgs e)
        {
            if(connectionAttempts == -1)
            {
                connectionTimer.Stop();
            }
            else if (vpnControl.isConnected() != wantsConnect)
            {
                // try to reconnect
                if (wantsConnect)
                {
                    if (connectionAttempts >= 3)
                    {
                        wantsConnect = false;

                        this.notifyIcon.BalloonTipText = "Unable to connect with VPN";
                        this.notifyIcon.BalloonTipTitle = "Confirmed VPN";
                        this.notifyIcon.ShowBalloonTip(3);
                        setDisconnected();

                        connectionAttempts = 0;
                    }
                    else
                    {
                        setConnecting();
                        connectionAttempts++;
                    }
                }
                else
                {
                    setConnected();
                }
            }
            else if (vpnControl.isConnected() && vpnControl.isVerified()) // checkbox and actual checked vpn match
            {
                connectionAttempts = 0;
                //toggledToConnect = false;

                //bluePowerCenter.Visibility = Visibility.Visible; // sometimes missed

                if (isConnecting)
                {
                    setConnected();
                }

                if (Properties.Settings.Default.streamReroute)
                {
                    sReroute.StartRerouting(vpnControl);
                }
            }

            /*if(isConnecting && vpnControl.isConnected() && vpnControl.isVerified())
            {
                isConnecting = false;
                OnPropertyChanged("isConnecting");
                bluePowerCenter.Visibility = Visibility.Visible;

                connectionLabel.Content = "CONNECTED";
            }
            else if(isConnecting && !vpnControl.isConnected())
            {
                isConnecting = false;
                wantsConnect = false;

                blueCircle.Visibility = Visibility.Hidden;
                grayCircle.Visibility = Visibility.Visible;

                connectionLabel.Content = "DISCONNECTED";
            }*/
        }

        private void setConnected()
        {
            isConnecting = false;
            wantsConnect = true;
            OnPropertyChanged("isConnecting");
            bluePowerCenter.Visibility = Visibility.Visible;

            connectionLabel.Content = "CONNECTED";

            blueCircle.Visibility = Visibility.Visible;
            grayCircle.Visibility = Visibility.Hidden;

            new System.Threading.Thread(() =>
            {
                vpnControl.SetDNS();
            }).Start();
        }

        private void setDisconnected()
        {
            /*isConnecting = false;
            wantsConnect = false;

            bluePowerCenter.Visibility = Visibility.Hidden;

            blueCircle.Visibility = Visibility.Hidden;
            grayCircle.Visibility = Visibility.Visible;

            connectionLabel.Content = "DISCONNECTED";*/

            isConnecting = false;
            wantsConnect = false;
            OnPropertyChanged("wantsConnect");

            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                /* run your code here */
                vpnControl.disconnectFromVPN();

                vpnControl.UnsetDNS();
            }).Start();

            blueCircle.Visibility = Visibility.Hidden;
            grayCircle.Visibility = Visibility.Visible;
            bluePowerCenter.Visibility = Visibility.Hidden;

            connectionLabel.Content = "DISCONNECTED";
        }

        private void setConnecting()
        {
            wantsConnect = true;
            OnPropertyChanged("wantsConnect");
            isConnecting = true;
            OnPropertyChanged("isConnecting");

            if (!reconnect)
            {
                new System.Threading.Thread(() =>
                {
                    System.Threading.Thread.CurrentThread.IsBackground = true;
                /* run your code here */
                    vpnControl.connectToVPN();
                }).Start();
            }


            bluePowerCenter.Visibility = Visibility.Hidden;
            grayCircle.Visibility = Visibility.Hidden;
            blueCircle.Visibility = Visibility.Visible;

            connectionLabel.Content = "CONNECTING";
        }

        private void PopulateComboBox()
        {
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/US.png", UriKind.Absolute)), "United States - West"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/US.png", UriKind.Absolute)), "United States - East"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/GB.png", UriKind.Absolute)), "United Kingdom"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/IE.png", UriKind.Absolute)), "Ireland"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/DE.png", UriKind.Absolute)), "Germany"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/CA.png", UriKind.Absolute)), "Canada"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/JP.png", UriKind.Absolute)), "Japan"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/AU.png", UriKind.Absolute)), "Australia"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/KR.png", UriKind.Absolute)), "South Korea"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/SG.png", UriKind.Absolute)), "Singapore"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/IN.png", UriKind.Absolute)), "India"));
            Regions.Add(new ComboBoxItem(new BitmapImage(new Uri("pack://application:,,,/Confirmed VPN;component/Resources/BR.png", UriKind.Absolute)), "Brazil"));

            regionComboBox.ItemsSource = Regions;

            SetComboBoxSelection();
        }

        private void SetComboBoxSelection()
        {
            foreach (ComboBoxItem item in regionComboBox.Items)
            {
                if (item.Label == Properties.Settings.Default.region)
                {
                    if (regionComboBox.SelectedItem != item)
                    {
                        initialSelect = true;
                        regionComboBox.SelectedItem = item;
                    }
                    break;
                }
            }
        }

        bool initialSelect = false;
        bool reconnect = false;
        private void regionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialSelect)
            {
                initialSelect = false;
            }
            else
            {
                string newRegion = ((ComboBoxItem)regionComboBox.SelectedItem).Label;

                if (vpnControl.isConnected())
                {
                    // get animation going
                    wantsConnect = true;
                    OnPropertyChanged("wantsConnect");
                    isConnecting = true;
                    OnPropertyChanged("isConnecting");
                    bluePowerCenter.Visibility = Visibility.Hidden;
                }

                new System.Threading.Thread(() =>
                {
                    reconnect = false;
                    if (vpnControl.isConnected())
                    {
                        reconnect = true;
                        vpnControl.disconnectFromVPN();
                    }

                    vpnControl.switchServer(newRegion);

                    Properties.Settings.Default.region = newRegion;
                    Properties.Settings.Default.Save();

                    if (reconnect)
                    {
                        vpnControl.connectToVPN();
                        reconnect = false;
                    }
                }).Start();
            }
        }

        private void powerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnecting)
            {
                if (!wantsConnect)
                {
                    setConnecting();
                }
                else
                {
                    setDisconnected();
                }
            }
        }

        private void account_Clicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(ConfirmedConstants.apiAccount);
            System.Diagnostics.Process.Start(sInfo);
        }

        private void speedTest_Clicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(ConfirmedConstants.SpeedTestSite);
            System.Diagnostics.Process.Start(sInfo);
        }

        private void vpnSettings_Clicked(object sender, RoutedEventArgs e)
        {
            if (vpnSettings == null || !vpnSettings.IsLoaded)
            {
                vpnSettings = new VPNSettings(sReroute);
            }

            vpnSettings.Show();
            vpnSettings.WindowState = WindowState.Normal;
            vpnSettings.Activate();
            vpnSettings.Focus();
        }

        private void signOut_Clicked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.password = "";
            Properties.Settings.Default.username = "";
            Properties.Settings.Default.Save();

            //connectionTimer.Stop();
            connectionAttempts = -1;

            new System.Threading.Thread(() =>
            {
                if (vpnControl.isConnected())
                {
                    vpnControl.disconnectFromVPN();
                    vpnControl.UnsetDNS();
                }

                vpnControl.removeVPN();
            }).Start();

            notifyIcon.Visible = false;
            this.Hide();

            if (vpnSettings != null && vpnSettings.IsLoaded)
            {
                vpnSettings.Close();
            }

            handleLoginForm(true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            shuttingProcedure();
            base.OnClosing(e);
        }

        private void exit_Clicked(object sender, RoutedEventArgs e)
        {
            shutItDown();
        }

        private void shutItDown()
        {
            //shuttingProcedure();
            this.Close();
        }

        private async void displayShutdownMessage()
        {
            notifyIcon.BalloonTipTitle = "Confirmed VPN";
            notifyIcon.BalloonTipText = "VPN disconnecting";
            notifyIcon.ShowBalloonTip(3);

            // need this for message to display
            await Task.Delay(1000);
        }

        bool isShuttingDown = false;
        private void shuttingProcedure()
        {
            if (!isShuttingDown)
            {
                isShuttingDown = true;
                if (vpnControl.isConnected())
                {
                    displayShutdownMessage();

                    vpnControl.disconnectFromVPN();
                    vpnControl.UnsetDNS();
                }

                vpnControl.removeVPN();
            }
        }
    }
}
