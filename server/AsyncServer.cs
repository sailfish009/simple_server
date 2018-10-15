using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace server
{

    class AsyncServer
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static NetworkStream stream = null;
        private static Socket server = null;

        private static bool isconnected { get; set; } = false;

        static bool SocketConnected(Socket s)
        {
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        public static void init()
        {
            var size = 1024;
            var buffer = new byte[size];
            // Task.Run(async () =>
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (isconnected)
                    {
                        if(SocketConnected(server))
                        {
                            try
                            {
                                if (stream.DataAvailable)
                                {
                                    var bytesRead = server.Receive(buffer);
                                    var actualData = new byte[bytesRead];
                                    Array.Copy(buffer, actualData, bytesRead);
                                    OnDataReceived(actualData);
                                }
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            isconnected = false;
                            allDone.Set();
                            Console.WriteLine("[server] closed");
                        }
                    }
                }
            });
        }

        static void OnDataReceived(byte[] data)
        {
            Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[server] received : {0}", Encoding.Default.GetString(data));
            });
        }


        public static void StartListening()
        {
            string address = "192.168.0.100";
            IPAddress ipAddress = IPAddress.Parse(address);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5555);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    Console.WriteLine("server 1");
                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                    Console.WriteLine("server 2");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            // allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            // Socket handler = listener.EndAccept(ar);
            server = listener.EndAccept(ar);

            isconnected = true;
            stream = new NetworkStream(server);
        }


        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void send(int id, byte[] data)
        {
            server.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), server);
        }

        public static void close()
        {
            // Release the socket.  
            try
            {
                stream.Close();
                server.Shutdown(SocketShutdown.Both);
                server.Close();
                server = null;
                isconnected = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


    }
}
