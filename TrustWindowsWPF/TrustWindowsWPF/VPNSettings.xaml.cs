using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TrustWindowsWPF
{
    /// <summary>
    /// Interaction logic for VPNSettings.xaml
    /// </summary>
    public partial class VPNSettings : Window
    {
        StreamReroute sReroute;

        public VPNSettings(StreamReroute reroute)
        {
            InitializeComponent();

            sReroute = reroute;

            whiteListCheckBox.IsChecked = Properties.Settings.Default.streamReroute;
            if (!Properties.Settings.Default.streamReroute)
            {
                whitelistGrid.IsEnabled = false;
            }
            netflixCheckBox.IsChecked = Properties.Settings.Default.rerouteNetflix;
            huluCheckBox.IsChecked = Properties.Settings.Default.rerouteHulu;

            PopulateList();
        }

        void PopulateList()
        {
            urlListBox.ItemsSource = Properties.Settings.Default.rerouteURLs;
        }

        private void addURLButton_Click(object sender, RoutedEventArgs e)
        {
            if (urlToAddTextBox.Text != "")
            {
                string newURL = urlToAddTextBox.Text;
                urlToAddTextBox.Text = "";

                if (!Properties.Settings.Default.rerouteURLs.Contains(newURL))
                {
                    Properties.Settings.Default.rerouteURLs.Add(newURL);
                    Properties.Settings.Default.Save();
                    urlListBox.ItemsSource = null;
                    urlListBox.ItemsSource = Properties.Settings.Default.rerouteURLs;

                    sReroute.addRerouteItem(newURL);
                }
            }
        }

        private void removeURLButton_Click(object sender, RoutedEventArgs e)
        {
            int index = urlListBox.SelectedIndex;

            if (index > -1)
            {
                string urlToRemove = Properties.Settings.Default.rerouteURLs[index];

                Properties.Settings.Default.rerouteURLs.RemoveAt(index);
                Properties.Settings.Default.Save();
                urlListBox.ItemsSource = null;
                urlListBox.ItemsSource = Properties.Settings.Default.rerouteURLs;

                sReroute.removeRerouteItem(urlToRemove);
            }
        }

        private void netflixCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.rerouteNetflix = netflixCheckBox.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.Save();

            if (netflixCheckBox.IsChecked.GetValueOrDefault())
            {
                sReroute.RerouteNetflix();
            }
            else
            {
                sReroute.endNetflixReroute();
            }
        }

        private void whitelistEnableCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            whitelistGrid.IsEnabled = whiteListCheckBox.IsChecked.GetValueOrDefault();

            Properties.Settings.Default.streamReroute = whiteListCheckBox.IsChecked.GetValueOrDefault();
        }

        private void huluCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.rerouteHulu = huluCheckBox.IsChecked.GetValueOrDefault();
            Properties.Settings.Default.Save();

            if (huluCheckBox.IsChecked.GetValueOrDefault())
            {
                sReroute.RerouteHulu();
            }
            else
            {
                sReroute.endHuluReroute();
            }
        }
    }
}
