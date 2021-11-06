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
using System.Xml.Serialization;

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

        private Thread thread;

        private List<Socket> _clientSockets = new List<Socket>();

        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        byte[] bufferData = new byte[1024];

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
                Interval = new TimeSpan(0, 0, 10)
            };

            timerIp.Tick += TimerIpOnTick;

            timerIp.Start();

            //thread = new Thread(new ThreadStart(ServerStart));
            //thread.IsBackground = true;
            //thread.Start();

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
                string publicIP = GetPublicIPAddress();

                if (!string.IsNullOrEmpty(publicIP))
                {
                    lbServerIp.Content = GetPublicIPAddress();
                }
            }

        }

        private void ServerStart()
        {
            //Initialize server

            //convert data to byte to send over ethernet
            ASCIIEncoding encoding = new ASCIIEncoding();

            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 90));

            _serverSocket.Listen(10);

            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);


            ////server alway run
            ////while (true)
            ////{
            //Socket socket = server.AcceptSocket();

            //lvIpConnect.Items.Add(IPAddress.Parse(socket.LocalEndPoint.ToString()).Address.ToString() + "Reques sycn time at " + DateTime.Now.ToString("dd/mm/yyyy HH:mm:ss"));

            ////receive request time sync from client 
            //socket.Receive(data);

            ////save time receive message sync time from client by time UTC
            //DateTime receive = DateTime.UtcNow;


            ////delay 1 seconds
            //Thread.Sleep(1000);

            ////send time receive
            //socket.Send(encoding.GetBytes(receive.ToString()));

            ////send time send message time receive
            //socket.Send(encoding.GetBytes(DateTime.UtcNow.ToString()));


            ////end sycn time
            //socket.Close();
            ////}

            //server.Stop();

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            _clientSockets.Add(socket);

            lvIpConnect.Dispatcher.Invoke(DispatcherPriority.Render, new Action(delegate ()
                {
                    lvIpConnect.Items.Add(socket.RemoteEndPoint.ToString() + " " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                }));

            socket.BeginReceive(bufferData, 0, bufferData.Length, SocketFlags.None, new AsyncCallback(ReceiceCallBack), socket);

            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private void ReceiceCallBack(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            int receiver = socket.EndReceive(ar);

            byte[] dataBuffer = new byte[receiver];

            Array.Copy(bufferData, dataBuffer, receiver);

            string text = Encoding.ASCII.GetString(dataBuffer);

            string timeReceiver = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss");

            Thread.Sleep(1000);

            byte[] dataSend = Encoding.ASCII.GetBytes(timeReceiver);

            socket.BeginSend(dataSend, 0, dataSend.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);

            dataSend = Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"));
            socket.BeginSend(dataSend, 0, dataSend.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
        }

        private void SendCallBack(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndSend(ar);
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            lbCurrentTime.Content = "Current Timer: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
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

            try
            {
                int first = address.IndexOf("Address: ") + 9;
                int last = address.LastIndexOf("</body>");
                address = address.Substring(first, last - first);
            }
            catch (Exception e)
            {

            }

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

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            //if (thread.IsAlive)
            //{
            //    thread.Abort();

            //}
        }
    }
}
