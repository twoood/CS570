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

    class Program
    {

        private static string ReachOut()
        {
            var ServerEp = new IPEndPoint(IPAddress.Any, 0);
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 2018);
            byte[] bytes = Encoding.ASCII.GetBytes("connect");
            client.Send(bytes, bytes.Length, ip);
            var ServerResponseData = client.Receive(ref ServerEp);
            var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);
            string the_response = ServerResponse;
            Console.WriteLine("Recived {0} from {1}", ServerResponse, ServerEp.Address.ToString());
            client.Close();
            return the_response;
        }

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

            string connection_information = ReachOut();

            while (!_clientSocket.Connected)
            {
                try
                {

                    //client is receiving IP from server
                    //now figure out how to connect using that IP

                    Console.WriteLine("In try loop " + connection_information);

                    string[] ipAndPort = connection_information.Split(':');
                    Console.WriteLine(ipAndPort[0]);
                    Console.WriteLine(ipAndPort[1]);

                    int port = Int32.Parse(ipAndPort[1]);

                    IPAddress serverIP = IPAddress.Parse(ipAndPort[0]);


                    Console.WriteLine("In try loop IP made: " + serverIP);

                    _clientSocket.Connect(serverIP, port);
                }
                catch (SocketException)
                {
                    //Console.Clear();
                    Console.WriteLine("Connection Attempts: " + attempts.ToString());
                }
            }

           // Console.Clear();
           // Console.WriteLine("Connected");
        }

        

        
    }
}
