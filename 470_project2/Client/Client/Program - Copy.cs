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
    public class SenderReceiver
    {
        UdpClient client = new UdpClient();
        IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 2018);
        public void Send(string msg)
        {

            byte[] bytes = Encoding.ASCII.GetBytes(msg);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }
    
        public int flag = 0;
        public string message = null;
       // private readonly UdpClient udp = new UdpClient(2018);
        public void StartListening()
        {
            this.client.BeginReceive(Receive, new object());
        }
        private void Receive(IAsyncResult ar)
        {
            Console.WriteLine("Receiver Started");
            //IPEndPoint ip = new IPEndPoint(IPAddress.Any, 2018);
            byte[] bytes = udp.EndReceive(ar, ref ip);
            message = Encoding.ASCII.GetString(bytes);
            Console.WriteLine(message);
            if (message != null)
            {
                // Found client
                flag = 1;
                Console.WriteLine("Int flag: " + flag);
                udp.Close();
                return;
            }
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
            Console.WriteLine("_clientSocket Remote end point: "+_clientSocket.RemoteEndPoint);

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

            attempts++;

            SenderReceiver receiverr = new SenderReceiver();

            sender.Send("connect");
            Console.WriteLine("connect sent: " + attempts.ToString());


           // Receiver receiver = new Receiver();
            receiver.StartListening();

           // Console.WriteLine("Received: " + receiver.message);
            while (receiver.flag != 1)
            {
                //wait for message verification
                // Console.WriteLine("IN FLAG != 1 LOOP");
            }

            while (!_clientSocket.Connected)
            {
                try
                {

                    //client is receiving IP from server
                    //now figure out how to connect using that IP

                    Console.WriteLine("In try loop " + receiver.message);

                    string[] ipAndPort = receiver.message.Split(':');

                    IPAddress serverIP = IPAddress.Parse(receiver.message);


                    Console.WriteLine("In try loop IP made: " + serverIP);

                    _clientSocket.Connect(serverIP, 2018);
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection Attempts: " + attempts.ToString());
                }
            }

           // Console.Clear();
           // Console.WriteLine("Connected");
        }

        

        
    }
}
