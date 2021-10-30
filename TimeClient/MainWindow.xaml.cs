﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
using System.Xml.Serialization;

namespace TimeClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<UTC> timeZone = new List<UTC>();

        public MainWindow()
        {
            InitializeComponent();

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

            cbTimeZone.SelectedIndex = 0;
            cbTimeServer.SelectedIndex = 0;
        }

        private void BtnSync_OnClick(object sender, RoutedEventArgs e)
        {
            if (cbTimeServer.SelectedIndex == 0)
            {
                tbStatus.Text = "Waiting for sync time from clock.tuitentuan.com";

                SyncTimeStatic();
            }
            else
            {
                if (string.IsNullOrEmpty(tbIpServer.Text))
                {
                    MessageBox.Show("Ip Server cannot be null");
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
                tbStatus.Text = "Cannot connect to server " + tbIpServer.Text;
            }

            SyncTime(IP);


        }

        private void SyncTime(IPAddress ip)
        {
            //Initialize client
            ASCIIEncoding encoding = new ASCIIEncoding();

            TcpClient client = new TcpClient();

            client.Connect(ip, 90);

            //if can connect to server
            if (client.Connected)
            {
                DateTime[] value = new DateTime[4];

                //save time send resquest
                value[0] = DateTime.UtcNow;

                byte[] data = encoding.GetBytes(value[0].ToString());

                Stream stream = client.GetStream();

                //send request sync time
                stream.Write(data, 0, data.Length);

                byte[] receive1 = new byte[1024];
                byte[] receive2 = new byte[1024];

                //read message send from server (time server receive and time server send message
                stream.Read(receive1, 0, 1024);
                stream.Read(receive2, 0, 1024);

                //save time receive message from server
                value[3] = DateTime.UtcNow;

                value[1] = DateTime.Parse(encoding.GetString(receive1));
                value[2] = DateTime.Parse(encoding.GetString(receive2));


                //caculator ofset time
                TimeSpan time = (value[1] - value[0]) + (value[2] - value[3]);

                int timeOfset = time.Milliseconds / 2;

                //add ofset time
                DateTime trueTime = DateTime.Now.AddMilliseconds(timeOfset);

                //set time to system
                SetTimeForSystem(trueTime);

                tbStatus.Text = "Sync time success from server" + ip + " at" + trueTime.ToString();
            }
            //cannot connect to server
            else
            {
                tbStatus.Text = "Cannot connect to server " + tbIpServer.Text;
            }

            client.Close();

        }

        private void SyncTimeStatic()
        {
            IPAddress baseIp = Dns.GetHostAddresses(@"clock.tuitentuan.com")[0];

            SyncTime(baseIp);
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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        private void SetTimeForSystem(DateTime time)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = (short)time.Year; // must be short
            st.wMonth = (short)time.Month;
            st.wDay = (short)time.Day;
            st.wHour = (short)time.Hour;
            st.wMinute = (short)time.Minute;
            st.wSecond = (short)time.Second;

            SetSystemTime(ref st);
        }
    }
}
