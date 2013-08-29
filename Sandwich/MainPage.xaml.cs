using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Sandwich.Resources;
using Microsoft.Phone.Net.NetworkInformation;
using Sandwich.Models;
using Newtonsoft.Json;

namespace Sandwich
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            peers = new List<Peer>();
            peerList.ItemsSource = peers;
        }

        List<Peer> peers;

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //if (!App.ViewModel.IsDataLoaded)
            //{
            //    App.ViewModel.LoadData();
            //}
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            string server = "sandstorm.case.edu";
            DnsEndPoint endpoint = new DnsEndPoint(server, 0);
            DeviceNetworkInformation.ResolveHostNameAsync(endpoint, OnNameResolved, null);
        }

        private void OnNameResolved(NameResolutionResult result)
        {
            if (result.NetworkErrorCode == NetworkError.Success)
            {
                IPEndPoint[] endpoints = result.IPEndPoints;
                IPAddress address = endpoints[0].Address;

                int port = GetPortFromAddress(address);
                System.Diagnostics.Debug.WriteLine(port);
                string url = "http://sandstorm.case.edu:" + port;

                var client = new WebClient();
                client.DownloadStringCompleted += client_DownloadStringCompleted;
                client.DownloadStringAsync(new Uri(url + "/peerlist"));
            }
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                peers = JsonConvert.DeserializeObject<List<Peer>>(e.Result);
                Dispatcher.BeginInvoke(delegate
                {
                    peerList.ItemsSource = null;
                    peerList.ItemsSource = peers;
                });
            }
        }

        private int GetPortFromAddress(IPAddress address)
        {
            byte[] addr;
            int port;

            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                addr = new byte[address.GetAddressBytes().Length + 12];
                Buffer.BlockCopy(address.GetAddressBytes(), 0, addr, 12, address.GetAddressBytes().Length);
                addr[10] = (byte)0xFF;
                addr[11] = (byte)0xFF;
            }
            else
            {
                addr = address.GetAddressBytes();
            }

            byte[] hash = MD5Core.GetHash(addr);

            uint part0 = hash[0];
            uint part1 = hash[1];
            uint part2 = hash[2];
            uint part3 = hash[3];

            port = (int)((part0 + part3) << 8);
            port = (int)(port + part1 + part2);
            port &= 0xFFFF;

            if (port < 1024)
            {
                port += 1024;
            }

            return port;
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}