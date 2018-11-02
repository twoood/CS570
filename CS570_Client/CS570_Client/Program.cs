﻿/*
 * Tom Wood
 * CS 570 
 * Client for multiple client server
 * I followed along with the following example to accomplish this project: www.youtube.com/watch?v=xgLRe7QV6QI
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Timers;

namespace CS570_Client
{
    class Program
    {
        private static Socket mysock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static System.Timers.Timer aTimer;
        private static byte[] rec_buff = new byte[1024];
        private static int rec_n = 0;
        private byte[] rec_msg = new byte[1024];





        static void Main(string[] args)
        {
            Console.Title = "Client";
            ReachOut();
            ConstantConnect();// constantly connect loop
            SendMessage();// send message loop
            //Console.Read();
        }

        private static void SendMessage()
        {
            // setup new timer to 1 second
            aTimer = new System.Timers.Timer(1000);
            while (true)
            {

                // Hook up CheckRetrieve for the timer, so it will be constantly called. 
                aTimer.Elapsed += CheckRetrieve;
                aTimer.AutoReset = true;
                aTimer.Enabled = true;



                // Ask for user to grant input, if they enter nothing, keep allowing input
                Console.WriteLine("Enter a message, specify a client id in order to send message to client, or enter q to quit:");
                string message = Console.ReadLine();
                while (message.Length == 0)
                {
                    message = Console.ReadLine();
                }
                // if q is selected, disconnect client
                if (message == "q")
                    Disconnect();
                byte[] buffer = Encoding.ASCII.GetBytes(message);// encode message into byte array
                mysock.Send(buffer, buffer.Length, SocketFlags.None);// send the byte buffer to server



            }
        }
        // check for data in the buffer
        private static void CheckRetrieve(Object source, ElapsedEventArgs e)
        {
            try
            {
                rec_n = mysock.Receive(rec_buff);// try to receive data in the globally defined buffer and store it in the int rec_n
            }
            catch (System.Net.Sockets.SocketException sockex) // if socket exception, print exception, and try to reconnect
            {
                int errCode = sockex.ErrorCode;
                Console.WriteLine(errCode.ToString());
                ConstantConnect();

            }
            if (rec_n > 0)// if data was received, then convert the byte array to a string and print output
            {
                byte[] rec_data = new byte[rec_n];
                Array.Copy(rec_buff, rec_data, rec_n);
                Console.WriteLine(Encoding.ASCII.GetString(rec_data));
            }
            return;
        }
        // function to constantly try to connect the client, and will loop until client is connected
        private static void ConstantConnect()
        {
            int connection_attempts = 0;
            while (!mysock.Connected)
            {

                try
                {
                    connection_attempts++;
                    mysock.Connect(IPAddress.Loopback, 7000);// hardcoded the loopback address, same port as server
                }
                catch (SocketException) // if doesn't successfully connect, then print the number of connection attempts
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts: " + connection_attempts.ToString());

                }
            }
            Console.Clear();
            Console.WriteLine("Client successfully connected");
        }

        // method to safely disconnect client and close the application
        private static void Disconnect()
        {
            mysock.Shutdown(SocketShutdown.Both);
            Environment.Exit(0);
        }

        private static void ReachOut()
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 2018);
            byte[] bytes = Encoding.ASCII.GetBytes("connect");
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }
    }
}
