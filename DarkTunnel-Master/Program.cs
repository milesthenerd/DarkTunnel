using System;

namespace DarkTunnel.Master
{
    static class Program
    {
        private static int port = 16702;
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (!int.TryParse(args[0], out port))
                    Console.WriteLine($"Unable to parse {args[0]} as a port number.");
            }

            MasterServer masterServer = new MasterServer(port);
            Console.WriteLine($"Listening on port {port}");

            Console.CancelKeyPress += (s, e) => { Quit(masterServer); };

            Console.WriteLine("Press q or ctrl+c to quit.");
            bool hasConsole = true;
            bool running = true;
            while (running)
            {
                if (!hasConsole)
                    continue;

                try
                {
                    ConsoleKeyInfo cki = Console.ReadKey(false);
                    if (cki.KeyChar == 'q') running = false;
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Program does not have a console, not listening for console input.");
                    hasConsole = false;
                }
            }
        }

        private static void Quit(MasterServer masterServer)
        {
            masterServer.Stop();
        }
    }
}