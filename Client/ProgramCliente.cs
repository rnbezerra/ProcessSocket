using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ProgramCliente
    {
        public static void Main(string[] args)
        {
            // Data buffer for incoming data.
            byte[] bytes = new byte[1024];

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
                    Console.WriteLine();
                    // Receive the response from the remote device.
                    Thread threadServerResponse = new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                            while (true)
                            {
                                if (!sender.Connected) break;
                                int bytesRec = sender.Receive(bytes);
                                Console.Write(Encoding.ASCII.GetString(bytes, 0, bytesRec));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("\nO cliente parou de ouvir o servidor.");
                        }
                    }));
                    threadServerResponse.Start();

                    //Console.Write("Digite um comando para iniciar: ");
                    while (true)
                    {
                        string consoleInput = Console.ReadLine();
                        // Encode the data string into a byte array.
                        byte[] msg = Encoding.UTF8.GetBytes(string.Format("{0}<eol>", consoleInput));
                                                
                        // Send the data through the socket.
                        int bytesSent = sender.Send(msg);

                        if (consoleInput.ToLower().IndexOf("exit") > -1)
                        {
                            threadServerResponse.Interrupt();
                            break;
                        }


                        
                    }


                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
    }
}
