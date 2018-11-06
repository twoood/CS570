/*
 * Tom Wood
 * CS 570 
 * Server for multiple client server
 * I followed along with the following example to accomplish this project: www.youtube.com/watch?v=xgLRe7QV6QI
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CS570_Server
{
    class Program
    {
        private static byte[] buff = new byte[1024];
        private static List<_con_client> _clientList = new List<_con_client>();
        private static Socket primary_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly UdpClient udp = new UdpClient(2018);
        
        
        

        
        private static void UDP_Server()
        {
            var Server = new UdpClient(2018);
            string ip = "127.0.0.4";
            var ResponseData = Encoding.ASCII.GetBytes(ip + ":" + 7000);
            string ClientRequest;
            do 
            {
                var ClientEp = new IPEndPoint(IPAddress.Any, 0);
                var ClientRequestData = Server.Receive(ref ClientEp);
                ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

                Console.WriteLine("Received {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
                Server.Send(ResponseData, ResponseData.Length, ClientEp);
            }
            while (ClientRequest != "connect");
        }


  

        static void Main(string[] args)
        {
            Console.Title = "Multiple Client Server";
                    
            UDP_Server();
            InitializeServer();
            
            Console.ReadLine();

        }

        private static void InitializeServer()
        {
            string my_ip = "127.0.0.4";
            IPAddress the_ip = IPAddress.Parse(my_ip);
            Console.WriteLine("Server Initialization...");
            primary_socket.Bind(new IPEndPoint(the_ip, 7000));   // bind primary socket to any ip address with port 7000

            primary_socket.Listen(4);   // Listen with a max of 4 queued clients

            primary_socket.BeginAccept(ServerAcceptCallback, null);    // begin accepting connections
            Console.WriteLine("Server is ready for client connections...");

        }
        // function to accept new clients
        private static void ServerAcceptCallback(IAsyncResult AR)
        {
            // new random number to generate id for client
            Random random = new Random();
            int randomNumber = random.Next(100, 1000); // RNG to get a value between 100 and 999
            Socket a_sock;
            try
            {
                a_sock = primary_socket.EndAccept(AR); // end Accepting socket and store accepted socket to a_sock 
                _con_client temp = new _con_client();  // create temporary _con_client object to store all the data for this client
                temp.id = randomNumber; // store random id assigned to client
                temp.ip = ((IPEndPoint)(a_sock.RemoteEndPoint)).Address.ToString(); // store ip address of client
                temp._c_sock = (Socket)a_sock;  // store socket of client
                string welcome = "Welcome, Client: " + temp.id;  // welcome string for client
                ClientToClient(welcome, a_sock); // sent welcome string to client
   
                _clientList.Add(temp);  // add this new _con_client to list
                Console.WriteLine("\nNew Client : " + temp.id.ToString() + " " + temp.ip);    // print to server the id and ip of new client

            }
            catch
            {
                return;
            }

            Array.Clear(buff, 0, buff.Length); // clear the buffer
            a_sock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, ServerReceiveCallback, a_sock); // begin receiveing transmission from socket
            primary_socket.BeginAccept(ServerAcceptCallback, primary_socket); // begin accepting connections again
        }

       // function for asynchoronous receiveing from client
        private static void ServerReceiveCallback(IAsyncResult AR)
        {
            // create a new _con_client object to keep track of the current socket's client information
            _con_client client = new _con_client();
            int cid;
            string cip;
            Socket r_sock = (Socket)AR.AsyncState; // get socket
            SocketError errorCode;  // SocketError code that could potentially be generated
            int d_rec;  // integer value to keep track of how many bytes of data we will receive from client

            try
            {
                d_rec = r_sock.EndReceive(AR, out errorCode); // amount of data received from client socket
                client = _clientList.Find(x => x._c_sock == r_sock);    // get this client from our list of clients
                cid = client.id;    // get this client's id
                cip = client.ip;    // get this client's ip address

            }
            catch (SocketException)
            {
                RemoveClientSocket(r_sock);// remove socket if exception is found
                return;
            }
            if (errorCode != SocketError.Success) // set buffer to 0 in the case there's a recoverable exception
            {
                d_rec = 0;
            }


            byte[] d_buff = new byte[d_rec];    //create byte buffer of the right size from the client's message
            Array.Copy(buff, d_buff, d_rec);    // copy onlt the bytes we care about into the new d_buff

            string text = Encoding.ASCII.GetString(d_buff); // convert d_buff into a string
            //Console.WriteLine("Text :" + text);
            if (text.Length == 1 && text == "q")    // if q is entered, the client will be disconnected
            {
                RemoveClientSocket(r_sock); // remove client socket from server
                return;
            }
            // this block of code checks to see if a valid id was input by the client, if so, it will search the List 
            // for the client specified by id and route the message to them. 
            int string_length = text.Length;
            if (string_length > 2) // check the length of string
            {
                string s_text = text.Substring(0, 3);
                int end = string_length- 3;
                bool isKey = s_text.All(char.IsDigit);// make sure the extracted substring is an integer
                string true_message = text.Substring(3, end);

                
                if (isKey == true)
                {
                    int key = Int32.Parse(s_text);
                    _con_client receiver = new _con_client();
                    receiver = _clientList.Find(x => x.id == key);// find client based on parsed id
                    if (receiver != null)
                    {
                        string client_message = "\nMessage from: " + cid + " " + cip + "\n" + true_message;
                        Console.WriteLine(client_message + "\n" + "Message for: " + receiver.id + " ip: " + receiver.ip);
                        ClientToClient(client_message, receiver._c_sock);
                    }
                    else
                    {
                        string loop_text = "Sorry that client doesn't exist\n";
                        string echo_text = loop_text + "Echo: " + text;
                        ClientToClient(echo_text, r_sock);
                    }
                }
                else
                {
                    Console.WriteLine("\nNot a client-to-client Message");
                    Console.WriteLine("Message from: " + cid + " ip: " + cip + "\n" + text);
                }
            }
            else // string length less than 2, so short message, not intended for another client
            {
                Console.WriteLine("Message from: " + cid + " ip: " + cip + "\n" + text);
                string s_data = "Hello from the server, " + cid + " enter q to disconnect\n";
                byte[] data = Encoding.ASCII.GetBytes(s_data);
                try
                {
                    r_sock.BeginSend(data, 0, data.Length, SocketFlags.None, SeverSendCallback, r_sock);
                }
                catch (System.Net.Sockets.SocketException sockex)
                {
                    int errCode = sockex.ErrorCode;
                    Console.WriteLine(errCode.ToString());
                    RemoveClientSocket(r_sock);
                    return;
                }
                
            }
            try// try to receive data on socket
            {
                r_sock.BeginReceive(buff, 0, buff.Length, SocketFlags.None, ServerReceiveCallback, r_sock);
            }
            catch (System.Net.Sockets.SocketException sockex)// if error, then print the correct error code
            {
                int errCode = sockex.ErrorCode;
                Console.WriteLine(errCode.ToString());
            }
        }


        // function to remove the client socket and delete it from the list of _con_client
        private static void RemoveClientSocket(Socket remove_sock)
        {
            remove_sock.Shutdown(SocketShutdown.Both);
            remove_sock.Close();
            _con_client _remove = new _con_client();
            _remove = _clientList.Find(x => x._c_sock == remove_sock);
            Console.WriteLine("Client " + _remove.id + " Disconnected");
            _clientList.Remove(_remove);
        }
        
        // function to asynchronously end the send of data on the particular socket 
        private static void SeverSendCallback(IAsyncResult AR)
        {
            Socket sock = (Socket)AR.AsyncState;
            sock.EndSend(AR);
        }

        // function to sent strings to a specified socket
        private static void ClientToClient(string input, Socket receive)
        {
            byte[] data = Encoding.ASCII.GetBytes(input);
            receive.BeginSend(data, 0, data.Length, SocketFlags.None, SeverSendCallback, receive);
            return;
        }


        
    }
    // a simple class to encapsulate all the data that we care about
    // a random id, the ip address, and the socket itself
    class _con_client
    {
        public int id { get; set; }
        public string ip { get; set; }
        public Socket _c_sock { get; set; }

    }

}
