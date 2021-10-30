using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace TimeServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private DispatcherTimer timer;

        private DispatcherTimer timerIp;

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, 200),
                IsEnabled = true
            };

            timer.Tick += TimerOnTick;

            timer.Start();

            timerIp = new DispatcherTimer()
            {
                IsEnabled = true,
                Interval = new TimeSpan(0, 0, 1)
            };

            timerIp.Tick += TimerIpOnTick;

            timerIp.Start();

            ServerStart();
        }

        private void TimerIpOnTick(object sender, EventArgs e)
        {
            if (ckbLanIp.IsChecked == true)
            {
                lbServerIp.Content = GetPrivateIPAddress();
            }
            else
            {
                lbServerIp.Content = GetPublicIPAddress();
            }

        }

        private void ServerStart()
        {
            //Initialize server

            //convert data to byte to send over ethernet
            ASCIIEncoding encoding = new ASCIIEncoding();

            TcpListener server = new TcpListener(IPAddress.Any, 90);

            server.Start();

            byte[] data = new byte[1024];


            //server alway run
            while (true)
            {
                Socket socket = server.AcceptSocket();

                lvIpConnect.Items.Add(IPAddress.Parse(socket.LocalEndPoint.ToString()).Address.ToString() + "Reques sycn time at " + DateTime.Now.ToString("dd/mm/yyyy HH:mm:ss"));

                //receive request time sync from client 
                socket.Receive(data);

                //save time receive message sync time from client by time UTC
                DateTime receive = DateTime.UtcNow;


                //delay 1 seconds
                Thread.Sleep(1000);

                //send time receive
                socket.Send(encoding.GetBytes(receive.ToString()));

                //send time send message time receive
                socket.Send(encoding.GetBytes(DateTime.UtcNow.ToString()));


                //end sycn time
                socket.Close();
            }

            server.Stop();

        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            lbCurrentTime.Content = "Current Timer: " + DateTime.Now.ToString("dd/mm/yyyy HH:mm:ss");
        }

        private string GetPublicIPAddress()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");

            try
            {
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    address = stream.ReadToEnd();
                }
            }
            catch (Exception e)
            {

            }

            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);

            return address;
        }

        private string GetPrivateIPAddress()
        {
            string localIP = string.Empty;

            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
            }
            catch (Exception e)
            {

            }

            return localIP;
        }

    }
}
