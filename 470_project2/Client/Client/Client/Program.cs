//Jared Wilson Project 1
//socket client code can run multiple instances

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Multiple_Socket_Client
{
    public class Sender
    {
        public void Send(string msg)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 2018);
            byte[] bytes = Encoding.ASCII.GetBytes(msg);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }
    }
    public class Receiver
    {
        private readonly UdpClient udp = new UdpClient(2018);
        public void StartListening()
        {
            this.udp.BeginReceive(Receive, new object());
        }
        private void Receive(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 2018);
            byte[] bytes = udp.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            Console.WriteLine(message);
            StartListening();
        }
    }
    class Program
    {
        //set up client socket
        private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        static void Main(string[] args)
        {
            //get client going
            Console.Title = "Client";

            LoopConnect();
            SendLoop();
            Console.ReadLine();
        }

        //loop communication with server
        private static void SendLoop()
        {
            Console.WriteLine("To send mesage to other client, enter their ID followed by message");
            Console.WriteLine("If message is waiting to be opened by client, hit enter on that client");

            while (true)
            {

                //add something tocheck receive buffer

                Console.WriteLine("Enter a request: ");
                string req = Console.ReadLine();
                byte[] buffer = Encoding.ASCII.GetBytes(req);
                _clientSocket.Send(buffer);

                byte[] receivedBuf = new byte[1024];
                int rec = _clientSocket.Receive(receivedBuf);
                byte[] data = new byte[rec];
                Array.Copy(receivedBuf, data, rec);
                Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
            }
        }

        //loop until client connects
        private static void LoopConnect()
        {
            int attempts = 0;

            while (!_clientSocket.Connected)
            {
                try
                {
                    attempts++;

                    Sender sender = new Sender();
                    //Receiver receiver = new Receiver();

                    sender.Send("connect");
                    Console.WriteLine("connect sent" + attempts.ToString());

                    _clientSocket.Connect(IPAddress.Loopback, 2018);
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection Attempts: " + attempts.ToString());
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

        

        
    }
}
