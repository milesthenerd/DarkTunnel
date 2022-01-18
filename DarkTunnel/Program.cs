using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DarkTunnel
{
    static class Program
    {
        private static NodeOptions options = new NodeOptions();
        public static void Main(string[] args)
        {
            //TODO: this whole project has a bunch of obsolete console.writelines
            if (!File.Exists("config.txt") && !TryCreateNewConfig())
                return;

            using (StreamReader sr = new StreamReader("config.txt"))
            {
                if (!options.Load(sr))
                {
                    Console.WriteLine("Failed to load config.txt");
                    return;
                }
            }

            TcpClient tcpClient = new TcpClient();
            UdpClient udpClient = new UdpClient(options.mediationClientPort);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                const int SIO_UDP_CONNRESET = -1744830452;
                udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
            }

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), options.localPort);
            MediationClient mediationClient = new MediationClient(tcpClient, udpClient, options.mediationIP, options.remoteIP, options.mediationClientPort, endpoint, options.isServer);

            mediationClient.TrackedClient();

            TunnelNode tn = new TunnelNode(options);

            if (options.isServer)
            {
                mediationClient.UdpServer();
                Console.WriteLine($"Server forwarding {options.endpoints[0]} to UDP port {options.localPort}");
                if (options.masterServerID != 0)
                    Console.WriteLine($"Server registering with master ID {options.masterServerID}");
            }
            else
            {
                mediationClient.UdpClient();
                Console.WriteLine($"Client forwarding TCP port {options.localPort} to UDP server {(options.masterServerID != 0 ? options.masterServerID : options.endpoints[0])}");
            }

            Console.WriteLine("Press q or ctrl+c to quit.");
            bool hasConsole = true;
            bool running = true;
            Console.CancelKeyPress += (s, e) => { running = false; tn.Stop(); };
            while (running)
            {
                if (!hasConsole)
                    continue;

                try
                {
                    ConsoleKeyInfo cki = Console.ReadKey(false);
                    if (cki.KeyChar == 'q')
                        running = false;
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Program does not have a console, not listening for console input.");
                    hasConsole = false;
                }
            }
        }

        /// <summary>
        /// Creates a new config.
        /// </summary>
        /// <returns>Returns `true` if creation was successful, `false` if creation was cancelled.</returns>
        private static bool TryCreateNewConfig()
        {
            Console.WriteLine("Unable to find config.txt");
            Console.WriteLine("Creating a default:");
            Console.WriteLine("c) Create a client config file");
            Console.WriteLine("s) Create a server config file");
            Console.WriteLine("Any other key: Quit");
            ConsoleKeyInfo cki = Console.ReadKey();
            switch (cki.KeyChar)
            {
                case 'c':
                {
                    options.isServer = false;
                    options.masterServerID = 0;
                    options.localPort = 5001;
                    using StreamWriter sw = new StreamWriter("config.txt");
                    options.Save(sw);
                    return true;
                }
                case 's':
                {
                    options.isServer = true;
                    options.endpoint = "127.0.0.1:25565";
                    options.localPort = 5001;
                    using StreamWriter sw = new StreamWriter("config.txt");
                    options.Save(sw);
                    return true;
                }
                default:
                {
                    Console.WriteLine("Quitting...");
                    return false;
                }
            }
        }
    }
}