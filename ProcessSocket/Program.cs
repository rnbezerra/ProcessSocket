using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread threadServer = new Thread(new ThreadStart(() =>
            {
                Server server = new Server();
                server.StartListening();
            }));
            
            Thread threadClient = new Thread(new ThreadStart(() =>
            {
                Client client = new Client();
                client.StartClient();
            }));

            threadServer.Start();
            threadClient.Start();

        }
    }
}
