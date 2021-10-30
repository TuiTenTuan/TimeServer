using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TimeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Network socket;

        private List<UTC> timeZone = new List<UTC>();

        public MainWindow()
        {
            InitializeComponent();

            socket = new Network();
        }

        private void CbTimeServer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTimeServer.SelectedIndex == 1)
            {
                lbIpServer.Visibility = Visibility.Visible;
                tbIpServer.Visibility = Visibility.Visible;
            }
            else
            {
                lbIpServer.Visibility = Visibility.Hidden;
                tbIpServer.Visibility = Visibility.Hidden;
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            lbIpServer.Visibility = Visibility.Hidden;
            tbIpServer.Visibility = Visibility.Hidden;

            tbStatus.Text = "";
            LoadTimeZone();

            cbTimeZone.ItemsSource = timeZone;
        }

        private void BtnSync_OnClick(object sender, RoutedEventArgs e)
        {
            if (cbTimeServer.SelectedIndex == 0)
            {
                tbStatus.Text = "Waiting for sync time from clock.tuitentuan.com";

                SyncTime();
            }
            else
            {
                if (string.IsNullOrEmpty(tbIpServer.Text))
                {
                    MessageBox.Show("Ip Server không được để trống");
                }
                else
                {
                    tbStatus.Text = "Waiting for sync time from " + tbIpServer.Text;
                    CheckIp();
                }
            }
        }

        private void CheckIp()
        {
            string ipinput = tbIpServer.Text;

            IPAddress IP;

            if (IPAddress.TryParse(ipinput, out IP))
            {
                tbStatus.Text = "Không thể kết nối đến server " + tbIpServer.Text;
            }
        }

        private void SyncTime()
        {
            IPAddress baseIp = Dns.GetHostAddresses(@"clock.tuitentuan.com")[0];




        }

        private void LoadTimeZone()
        {
            timeZone.Clear();

            string text = File.ReadAllText(Environment.CurrentDirectory + @"\UTCTimeZone.txt");

            string[] textline = text.Split('\n');

            foreach (string s in textline)
            {
                string utcValue = s.Split(' ')[0];
                string name = s.Substring(utcValue.Length);

                UTC utc = new UTC(utcValue, name);

                timeZone.Add(utc);
            }
        }
    }
}
