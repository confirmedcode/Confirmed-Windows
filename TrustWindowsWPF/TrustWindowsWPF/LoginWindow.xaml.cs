using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TrustWindowsWPF
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        [DllImport("wininet.dll")]
        static extern InternetCookieState InternetSetCookieEx(
            string lpszURL,
            string lpszCookieName,
            string lpszCookieData,
            int dwFlags,
            int dwReserved);

        enum InternetCookieState : int
        {
            COOKIE_STATE_UNKNOWN = 0x0,
            COOKIE_STATE_ACCEPT = 0x1,
            COOKIE_STATE_PROMPT = 0x2,
            COOKIE_STATE_LEASH = 0x3,
            COOKIE_STATE_DOWNGRADE = 0x4,
            COOKIE_STATE_REJECT = 0x5,
            COOKIE_STATE_MAX = COOKIE_STATE_REJECT
        }

        private APIControl apiControl;
        private bool isSignUp = true;

        bool isSubscribed;
        bool initialClick = false;

        public LoginWindow(APIControl apiControl)
        {
            InitializeComponent();

            this.apiControl = apiControl;

            if (Properties.Settings.Default.username != "" && Properties.Settings.Default.password != "")
            {
                emailTextBox.Text = Properties.Settings.Default.username;
                passwordBox.Password = DPAPIInterface.Unprotect(Properties.Settings.Default.password, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);

                passwordConfirmHide(true);
                signInButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else if (Properties.Settings.Default.username != "")
            {
                emailTextBox.Text = Properties.Settings.Default.username;

                passwordConfirmHide(true);
                signInButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
            else
            {
                passwordConfirmHide(false);
                signUpButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }

            initialClick = true;
            submitButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                submitButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            }
        }

        private void passwordConfirmHide(bool doHide)
        {
            if (doHide)
            {
                passwordConfirmBox.Visibility = Visibility.Hidden;
                forgotPasswordButton.Visibility = Visibility.Visible;
            }
            else
            {
                passwordConfirmBox.Visibility = Visibility.Visible;
                forgotPasswordButton.Visibility = Visibility.Hidden;
            }
        }

        private void signUpButton_Click(object sender, RoutedEventArgs e)
        {
            if(!isSignUp)
            {
                isSignUp = true;
                signUpButton.Foreground = (Brush)FindResource("ConfirmedBlue");
                signInButton.Foreground = (Brush)FindResource("CustomDarkGray");
                passwordConfirmHide(false);        
            }
        }

        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSignUp)
            {
                isSignUp = false;
                signUpButton.Foreground = (Brush)FindResource("CustomDarkGray");
                signInButton.Foreground = (Brush)FindResource("ConfirmedBlue");
                passwordConfirmHide(true);
            }
        }

        private void signOutButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.password = "";
            Properties.Settings.Default.Save();
            passwordBox.Password = "";
            isSubscribed = false;
            signInButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 0));
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        string username = null;
        string password = null;
        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (emailTextBox.Text != "" && passwordBox.Password != "" && (!isSignUp || passwordBox.Password == passwordConfirmBox.Password))
            {
                submitButton.IsEnabled = false;

                if (!initialClick)
                {
                    submitHideButton.Visibility = Visibility.Hidden; // hide enable animation on first appearance
                }

                username = password = null;

                new System.Threading.Thread(() =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        Properties.Settings.Default.username = username = emailTextBox.Text;
                        password = passwordBox.Password;
                        Properties.Settings.Default.password = DPAPIInterface.Protect(password, null, System.Security.Cryptography.DataProtectionScope.CurrentUser);
                        Properties.Settings.Default.Save();
                    });

                    Task<HttpResponseMessage> response = null;

                    while(username == null || password == null)
                    {
                        Thread.Sleep(10);
                    }

                    bool hasError = false;

                    // only create user in register
                    if (isSignUp)
                    {
                        if (!apiControl.checkVersion1Login(username, password))
                        {
                            response = apiControl.signUp(username, password);
                            var signUpResponseString = response.Result.Content.ReadAsStringAsync();

                            try
                            {
                                var jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(signUpResponseString.Result);

                                if (jsonDict.ContainsKey("code") && jsonDict["code"] == "3")
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        confirmEmailLabel.Content = "Password must contain a capital letter, contain a\nnumber, and contain a special character.";
                                        confirmEmailLabel.Visibility = Visibility.Visible;
                                        submitButton.IsEnabled = true;
                                    });

                                    hasError = true;
                                }
                            }
                            catch(AggregateException ae)
                            {

                            }
                        }
                    }

                    if (!hasError)
                    {
                        try
                        {
                            APIControl.SignInResponse signInResponse = apiControl.signIn(username, password);

                            if (signInResponse == APIControl.SignInResponse.AccountNotFound)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    confirmEmailLabel.Content = "Incorrect email or password.";
                                    confirmEmailLabel.Visibility = Visibility.Visible;
                                    submitButton.IsEnabled = true;
                                });
                            }
                            else if (signInResponse == APIControl.SignInResponse.AwaitingVerification)
                            {
                                this.Dispatcher.Invoke(() =>
                                        {
                                            confirmEmailLabel.Content = "Please confirm email.";
                                            confirmEmailLabel.Visibility = Visibility.Visible;
                                            submitButton.IsEnabled = true;
                                        });
                            }
                            else
                            {
                                var getKeyResponse = apiControl.getKey();
                                var getKeyResponseString = getKeyResponse.Result.Content.ReadAsStringAsync();

                                if (getKeyResponse.Result.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    var testDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(getKeyResponseString.Result);

                                    if (testDict.ContainsKey("code") && testDict["code"] == "1")
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            confirmEmailLabel.Content = "Please confirm email.";
                                            confirmEmailLabel.Visibility = Visibility.Visible;
                                            submitButton.IsEnabled = true;
                                        });
                                    }
                                    // else finished registration and subscription
                                    else if (!testDict.ContainsKey("code"))
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            isSubscribed = true;
                                            emailLabel.Content = Properties.Settings.Default.username;
                                            confirmEmailLabel.Visibility = Visibility.Hidden;
                                        //wizardPages1.SelectedTab = tabPage5;
                                        Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 2));
                                            submitButton.IsEnabled = true;
                                        });
                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            isSubscribed = false;
                                            confirmEmailLabel.Visibility = Visibility.Hidden;
                                            emailLabel.Content = Properties.Settings.Default.username;
                                            initBrowser();
                                        //wizardPages1.SelectedTab = tabPage5; // go to EULA after sign in page
                                        Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 2));
                                            submitButton.IsEnabled = true;
                                        });
                                    }
                                }
                                else if (getKeyResponse.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                {
                                    var testDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(getKeyResponseString.Result);
                                    if (testDict.ContainsKey("code") && testDict["code"] == "1")
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            confirmEmailLabel.Content = "Please confirm email.";
                                            confirmEmailLabel.Visibility = Visibility.Visible;
                                            submitButton.IsEnabled = true;
                                        });
                                    }
                                    else
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            confirmEmailLabel.Content = "Incorrect email or password.";
                                            confirmEmailLabel.Visibility = Visibility.Visible;
                                            submitButton.IsEnabled = true;
                                        });
                                    }
                                }
                                else if (getKeyResponse.Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                                {
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        confirmEmailLabel.Content = "Account does not include desktop access.\nUpgrade to unlimited.";
                                        confirmEmailLabel.Visibility = Visibility.Visible;
                                        submitButton.IsEnabled = true;
                                    });
                                }
                            }
                        }
                        catch (AggregateException ae)
                        {
                            MessageBox.Show("Can't connect to internet."); 

                            this.Dispatcher.Invoke(() =>
                            {
                                submitButton.IsEnabled = true;
                            });
                        }
                    }

                }).Start();
            }
            // else if passwords don't match
            else if (isSignUp && passwordBox.Password != "" && passwordBox.Password != passwordConfirmBox.Password)
            {
                confirmEmailLabel.Content = "Passwords do not match.";
                confirmEmailLabel.Visibility = Visibility.Visible;
                submitButton.IsEnabled = true;
            }
            else
            {
                submitButton.IsEnabled = true;
            }

            initialClick = false;
        }

        private void secureButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 1));
        }

        private void backSubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 0));
        }

        private void agreeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(agreeCheckBox.IsChecked.GetValueOrDefault())
            {
                agreeButton.IsEnabled = true;
            }
            else
            {
                agreeButton.IsEnabled = false;
            }
        }

        private void agreeBackButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 2));
        }

        private void agreeButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 4));
        }

        private void signedInContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSubscribed)
            {
                Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 5));
            }
            else
            {
                Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 3));
            }
        }

        private void forgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.ProcessStartInfo sInfo = new System.Diagnostics.ProcessStartInfo(ConfirmedConstants.apiForgotPassword);
            System.Diagnostics.Process.Start(sInfo);
        }

        private void initBrowser()
        {
            var locale = CultureInfo.CurrentCulture.Name;

            CookieCollection cookies = apiControl.getCookies().GetCookies(new Uri(ConfirmedConstants.apiAddress));

            foreach (Cookie cookie in cookies)
            {
                bool result = InternetSetCookie(ConfirmedConstants.apiAddress, null, cookie.ToString() + "; expires = " + cookie.Expires.ToString());
            }

            stripeBrowser.Navigate(ConfirmedConstants.apiNewSubscription + locale);
        }

        private void installButton_Click(object sender, RoutedEventArgs e)
        {
            var getKeyResponse = apiControl.getKey();
            var getKeyResponseString = getKeyResponse.Result.Content.ReadAsStringAsync();

            var getKeyDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(getKeyResponseString.Result);

            installButton.IsEnabled = false;
            installHideButton.Visibility = Visibility.Hidden;

            new System.Threading.Thread(() =>
            {

                if (getKeyDict.ContainsKey("b64"))
                {
                    string tempFile = System.IO.Path.GetTempFileName();

                    File.WriteAllBytes(tempFile, Convert.FromBase64String(getKeyDict["b64"]));

                    VPNControl.addCertificate(tempFile);

                    if (VPNControl.checkCertificateInstalled())
                    {
                        string defaultRegion = Properties.Settings.Default.region;

                        if (defaultRegion == "")
                        {
                            defaultRegion = "United States - West";

                            string regionCode = RegionInfo.CurrentRegion.TwoLetterISORegionName.ToUpper();

                            if (regionCode == "US")
                            {
                                TimeZone theTZ = TimeZone.CurrentTimeZone;

                                if (theTZ.StandardName == "Eastern Standard Time" || theTZ.StandardName == "Central Standard Time")
                                {
                                    defaultRegion = "United States - East";
                                }
                            }
                            if (regionCode == "GB")
                            {
                                defaultRegion = "United Kingdom";
                            }
                            if (regionCode == "IE")
                            {
                                defaultRegion = "Ireland";
                            }
                            if (regionCode == "CA")
                            {
                                defaultRegion = "Canada";
                            }
                            if (regionCode == "KO")
                            {
                                defaultRegion = "South Korea";
                            }
                            if (regionCode == "SG")
                            {
                                defaultRegion = "Singapore";
                            }
                            if (regionCode == "DE" || regionCode == "FR" || regionCode == "IT" || regionCode == "PT" || regionCode == "ES" ||
                                regionCode == "AT" || regionCode == "PL" || regionCode == "RU" || regionCode == "UA")
                            {
                                defaultRegion = "Germany";
                            }
                            if (regionCode == "AU" || regionCode == "NZ")
                            {
                                defaultRegion = "Australia";
                            }
                            if (regionCode == "JP")
                            {
                                defaultRegion = "Japan";
                            }
                            if (regionCode == "IN" || regionCode == "PK" || regionCode == "BD")
                            {
                                defaultRegion = "India";
                            }
                            if (regionCode == "BR" || regionCode == "CO" || regionCode == "VE" || regionCode == "AR")
                            {
                                defaultRegion = "Brazil";
                            }

                        }

                        //VPNControl.addVPN(defaultRegion);

                        Properties.Settings.Default.region = defaultRegion;
                        Properties.Settings.Default.Save();

                        this.Dispatcher.Invoke(() =>
                        {
                            this.Close();
                        });
                    }
                    else
                    {
                        // No Certificate found by that subject name.
                    }

                }
                else
                {
                    
                }

                this.Dispatcher.Invoke(() =>
                {
                    installButton.IsEnabled = true;
                });
            }).Start();
        }

        private void quitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void stripeBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if(e.Uri.AbsoluteUri.ToLower().Contains("tunnels://stripesuccess"))
            {
                Dispatcher.BeginInvoke((Action)(() => loginTabControl.SelectedIndex = 5));
            }
        }
    }

    public class PasswordBoxMonitor : DependencyObject
    {
        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached("IsMonitoring", typeof(bool), typeof(PasswordBoxMonitor), new UIPropertyMetadata(false, OnIsMonitoringChanged));



        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxMonitor), new UIPropertyMetadata(0));

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }

        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }
    }
}
