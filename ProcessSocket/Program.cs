using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessSocket
{
    public class Program
    {

        // Incoming data from the client.
        public static string data = null;

        public static void Main(string[] args)
        {

            //===============================================================================

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // Dns.GetHostName returns the name of the 
            // host running the application.
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                Socket handler = listener.Accept();

                Console.WriteLine("Connection established...");
                Console.WriteLine();
                //====================================================================
                //CRIAÇÃO DO PROCESSO
                Process p = new Process();
                ProcessStartInfo process = new ProcessStartInfo("cmd.exe");
                process.CreateNoWindow = true;
                process.UseShellExecute = false;
                process.RedirectStandardOutput = true;
                process.RedirectStandardInput = true;
                process.RedirectStandardError = true;

                p.StartInfo = process;

                p.Start();

                StreamReader reader = p.StandardOutput;
                StreamWriter writer = p.StandardInput;
                StreamReader readerErro = p.StandardError;

                //=====================================================================
                Thread thrRead = new Thread(new ThreadStart(() =>
                {
                    char[] buffer = new char[1024];
                    int totalRead = 0;
                    while (true)
                    {
                        totalRead = reader.Read(buffer, 0, buffer.Length);

                        string msg = new string(buffer, 0, totalRead);

                        Console.Write(msg);
                        byte[] msgToBeSent = Encoding.ASCII.GetBytes(msg);
                        handler.Send(msgToBeSent);

                        Thread.Sleep(100);
                    }
                }));

                thrRead.Start();
                //=====================================================================
                
                while (true)
                {
                    data = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.ToLower().IndexOf("<eol>") > -1)
                        {
                            data = data.Replace("<eol>", "");
                            break;
                        }
                    }


                    // Show the data on the console.
                    //Console.WriteLine("CLIENT>> {0}", data);
                    writer.Write(data + "\n");
                    writer.Flush();
                    if (data.ToLower().IndexOf("exit") > -1)
                    {
                        thrRead.Interrupt();
                        break;
                    }

                    // Echo the data back to the client.
                    //byte[] msg = Encoding.ASCII.GetBytes(data);
                    
                    //handler.Send(msg);
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                //FECHANDO PROCESSO
                reader.Close();
                writer.Close();
                readerErro.Close();
                p.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
        
        public static string ExecuteCMDCommand(String command)
        {
            Process p = new Process();
            ProcessStartInfo process = new ProcessStartInfo("cmd.exe");
            process.CreateNoWindow = true;
            process.UseShellExecute = false;
            process.RedirectStandardInput = true;
            process.RedirectStandardOutput = true;
            process.RedirectStandardError = true;

            p.StartInfo = process;

            p.Start();

            StreamReader reader = p.StandardOutput;
            StreamWriter writer = p.StandardInput;
            StreamReader readerError = p.StandardError;
            
            //CAPTURA OUTPUT DO CONSOLE
            StringBuilder consoleOutput = new StringBuilder();
            Thread thrRead = new Thread(new ThreadStart(() =>
            {
                char[] buffer = new char[1024];
                int totalRead = 0;
                while (true)
                {
                    totalRead = reader.Read(buffer, 0, buffer.Length);

                    string msg = new string(buffer, 0, totalRead);

                    //Console.Write(msg);
                    consoleOutput.Append(msg);
                    if (reader.EndOfStream) break;

                    Thread.Sleep(100);
                }
            }));

            thrRead.Start();

            //EXECUTA COMANDO
            writer.Write(command + "\n");
            writer.Flush();

            thrRead.Join();

            writer.Close();
            reader.Close();
            readerError.Close();
            p.Close();

            return consoleOutput.ToString();
        }
    }
}
