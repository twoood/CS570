//Jared Wilson CS470 Project 1
//socket server with multiple client capability

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;



namespace Multiple_Socket_Server
{


    class Program
    {

        private static void UDP_Server(string ip)
        {
            var Server = new UdpClient(2018);

            string ClientRequest;
            do
            {
                var ClientEp = new IPEndPoint(IPAddress.Any, 0);
                var ClientRequestData = Server.Receive(ref ClientEp);
                ClientRequest = Encoding.ASCII.GetString(ClientRequestData);
                var ResponseData = Encoding.ASCII.GetBytes(ip + ":" + 2018);
                Console.WriteLine("Received {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
                Server.Send(ResponseData, ResponseData.Length, ClientEp);
            }
            while (ClientRequest != "connect");
        }

        //buffer to use between functions
        private static byte[] _buffer = new byte[1024];
        //list of connected clients
        private static List<Socket> _clientSockets = new List<Socket>();
        //socket for server
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        static void Main(string[] args)
        {
            //get program started
            Console.Title = "Server";
            SetupServer();
            //make sure server does not quit unexpectedly
            Console.ReadLine();
        }
        //get server going
        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            Console.WriteLine("This is the server for Jared and Sara's Shop");

            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            string localIP = ipAddress.ToString();
            Console.WriteLine("IP Host: "+localIP);

            //new
            UDP_Server(localIP);



            //listen for a connection
            _serverSocket.Bind(new IPEndPoint(ipAddress, 2018));

            //Console.WriteLine(_serverSocket.RemoteEndPoint);



            _serverSocket.Listen(5);
            //accept client connect
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);

        }


        private static void AcceptCallback(IAsyncResult AR)
        {
            //new client connects
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(socket);
            //display that client has connected
            Console.WriteLine("Client Connected");
            Console.WriteLine("Client IP: " + socket.RemoteEndPoint);
            Console.WriteLine("Client Unique ID: " + socket.GetHashCode());

            string welcome = "\nWelcome to S&J Flannel Slang!\n-------------------------------\n";

            byte[] data = Encoding.ASCII.GetBytes(welcome);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);

            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            //recieve data from client
            Socket socket = (Socket)AR.AsyncState;

            //get  unique id to string
            int id = socket.GetHashCode();
            string stringID = id.ToString();


            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer, dataBuf, received);

            //display data
            string text = Encoding.ASCII.GetString(dataBuf);
            Console.Write("Text Received from " + socket.GetHashCode());
            Console.WriteLine(": " + text);
            Console.WriteLine("IP Address of Socket is: " + socket.RemoteEndPoint);

            //decide what to do with client response
            string response = string.Empty;

            //figure out if client included an Id in their message
            string resultString = Regex.Match(text, @"\d+").Value;


            //create array of socket id's
            //made this program to only support 5 unique clients but it could support as many as wanted
            int[] idList = new int[5] { 0, 0, 0, 0, 0 };

            //iterate thru each client
            foreach (Socket i in _clientSockets)
            {
                int a = 0;
                idList[a] = i.GetHashCode();

                //if ID in message is equal to another client go in to this statement
                if (idList[a].ToString() == resultString)
                {
                    Console.WriteLine(socket.GetHashCode() + " wants to connect to Client: " + resultString);
                    //response goes back to orig client
                    response = "You want to connect to Client: " + resultString;

                    //send message to socket i (the target)

                    //this w0rks but client needs to press enter to read
                    byte[] data2 = Encoding.ASCII.GetBytes("Message from " + socket.GetHashCode() + ": " + text);

                    i.Send(data2);

                    break;
                }
                else
                {
                    response = "S&J Flannel Slang heard you say: " + text;
                }


                a++;
            }

            byte[] data = Encoding.ASCII.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

        }


        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }




    }
}
