using NATTunnel.Common;
using System;

namespace NATTunnel;

internal static class Program
{
    public static void Main()
    {
        Tunnel.Start();

        while (Console.KeyAvailable == false)
        {

        }
        Console.WriteLine("NATTunnel exited.");
    }
}