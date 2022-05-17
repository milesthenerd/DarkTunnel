using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace NATTunnel.Common;

/// <summary>
/// Class to access configuration files.
/// </summary>
public static class Config
{
    #region Config Options

    /// <summary>
    /// Text string for "mode" in the config.
    /// </summary>
    private const string Mode = "mode";

    /// <summary>
    /// Text string for "client" in the config.
    /// </summary>
    private const string Client = "client";

    /// <summary>
    /// Text string for "server" in the config.
    /// </summary>
    private const string Server = "server";

    /// <summary>
    /// Text string for "endpoint" in the config.
    /// </summary>
    private const string Endpoint = "endpoint";

    /// <summary>
    /// Text string for "mediationIP" in the config.
    /// </summary>
    private const string MediationIp = "mediationIP";

    /// <summary>
    /// Text string for "remoteIP" in the config.
    /// </summary>
    private const string RemoteIp = "remoteIP";

    /// <summary>
    /// Text string for "localPort" in the config.
    /// </summary>
    private const string LocalPort = "localPort";

    /// <summary>
    /// Text string for "mediationClientPort" in the config.
    /// </summary>
    private const string MediationClientPort = "mediationClientPort";

    /// <summary>
    /// Text string for "uploadSpeed" in the config.
    /// </summary>
    private const string UploadSpeed = "uploadSpeed";

    /// <summary>
    /// Text string for "downloadSpeed" in the config.
    /// </summary>
    private const string DownloadSpeed = "downloadSpeed";

    /// <summary>
    /// Text string for "minRetransmitTime" in the config.
    /// </summary>
    private const string MinRetransmitTime = "minRetransmitTime";
    /// <summary>
    /// Text string for "connectionType" in the config.
    /// </summary>
    private const string ConnectionType = "connectionType";
    /// <summary>
    /// Text string for "tcp" in the config.
    /// </summary>
    private const string TCP = "tcp";
    /// <summary>
    /// Text string for "udp" in the config.
    /// </summary>
    private const string UDP = "udp";

    #endregion

    /// <summary>
    /// Tries to load the config file.
    /// </summary>
    /// <returns>Returns <see langword="true"/> if loading was successful, <see langword="false"/> if it wasn't.</returns>
    public static bool TryLoadConfig()
    {
        using StreamReader streamReader = new StreamReader(GetConfigFilePath());
        string currentLine;
        string tempEndpoint = "";
        while ((currentLine = streamReader.ReadLine()) != null)
        {
            // Skip all lines which don't have contents or are commented out.
            if ((currentLine.Length > 1) && (currentLine[0] == '#')) continue;

            // If the current line has no '=', or it's at the beginning, skip.
            int splitIndex = currentLine.IndexOf("=", StringComparison.Ordinal);
            if (splitIndex <= 0) continue;

            string leftSide = currentLine[..splitIndex].Trim();
            string rightSide = currentLine[(splitIndex + 1)..].Trim();
            switch (leftSide)
            {
                case Mode:
                    // If the Mode is neither Server nor Client, exit.
                    if (!(rightSide.Equals(Server) || rightSide.Equals(Client)))
                    {
                        Console.Error.WriteLine($"Unknown option '{rightSide}' for {Mode}!");
                        return false;
                    }
                    // Otherwise, assign IsServer
                    NodeOptions.IsServer = rightSide == Server;
                    break;
                case Endpoint:
                    // Temp memorize the endpoint for now, in order to resolve later when we have the mediation port.
                    try
                    {
                        tempEndpoint = rightSide;
                    }
                    catch
                    {
                        Console.Error.WriteLine($"Could not resolve '{rightSide}' to an IP address!");
                        return false;
                    }
                    break;
                case MediationIp:
                    // If no port is specified, error out.
                    int colonIndex = rightSide.IndexOf(':');
                    if (colonIndex <= 0)
                    {
                        Console.Error.WriteLine($"{MediationIp} must have a port specified!");
                        return false;
                    }

                    string ip = rightSide[..colonIndex];
                    string port = rightSide[(colonIndex + 1)..];
                    int portForMediationIP;

                    if (!Int32.TryParse(port, out portForMediationIP))
                    {
                        Console.Error.WriteLine($"Invalid port for {MediationIp}!");
                        return false;
                    }

                    try
                    {
                        NodeOptions.MediationIp = new IPEndPoint(GetIPFromDnsResolve(ip), portForMediationIP);
                    }
                    catch
                    {
                        Console.Error.WriteLine($"Could not resolve '{ip}' to an IP address!");
                        return false;
                    }
                    break;
                case RemoteIp:
                    // If the IP can't be resolved, error out.
                    try
                    {
                        NodeOptions.RemoteIp = GetIPFromDnsResolve(rightSide);
                    }
                    catch
                    {
                        Console.Error.WriteLine($"Could not resolve '{rightSide}' to an IP address!");
                        return false;
                    }
                    break;

                // If any of the values below can't be parsed, error out.
                case LocalPort:
                    if (!Int32.TryParse(rightSide, out NodeOptions.LocalPort))
                    {
                        Console.Error.WriteLine($"Invalid port for {LocalPort}");
                        return false;
                    }
                    break;
                case MediationClientPort:
                    if (!Int32.TryParse(rightSide, out NodeOptions.MediationClientPort))
                    {
                        Console.Error.WriteLine($"Invalid port for {MediationClientPort}");
                        return false;
                    }
                    break;
                case UploadSpeed:
                    if (!Int32.TryParse(rightSide, out NodeOptions.UploadSpeed))
                    {
                        Console.Error.WriteLine($"Invalid entry for {UploadSpeed}");
                        return false;
                    }
                    NodeOptions.UploadSpeed = Int32.Parse(rightSide);
                    break;
                case DownloadSpeed:
                    if (!Int32.TryParse(rightSide, out NodeOptions.DownloadSpeed))
                    {
                        Console.Error.WriteLine($"Invalid entry for {DownloadSpeed}");
                        return false;
                    }
                    NodeOptions.DownloadSpeed = Int32.Parse(rightSide);
                    break;
                case MinRetransmitTime:
                    if (!Int32.TryParse(rightSide, out NodeOptions.MinRetransmitTime))
                    {
                        Console.Error.WriteLine($"Invalid entry for {MinRetransmitTime}");
                        return false;
                    }
                    break;
                case ConnectionType:
                    if (!Enum.TryParse(rightSide, true, out NodeOptions.ConnectionType))
                    {
                        Console.Error.WriteLine($"Invalid entry for {ConnectionType}");
                        return false;
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown config option {leftSide}!");
                    break;
            }
        }
        // Before exiting, we need to give the correct port (mediation) to the endpoint.
        // If resolving fails, we return false, otherwise true.
        return ResolveAddressAndAssignToEndpoints(tempEndpoint, NodeOptions.MediationClientPort);
    }

    /// <summary>
    /// Helper method that resolves a DNS and returns the correct IPvX ip depending on what's supported.
    /// </summary>
    /// <param name="dns">The dns to resolve and get the ip from.</param>
    /// <returns>An IPv6 ip of IPv6 is supported, otherwise an IPv4 ip.</returns>
    private static IPAddress GetIPFromDnsResolve(string dns)
    {
        IPAddress[] ips = Dns.GetHostAddresses(dns);
        IPAddress ipToReturn = null;

        // If we support ipv6, return the first ipv6 ip (if it exists), otherwise return the first ipv4 ip.
        if (NodeOptions.IsIPv6Supported)
            ipToReturn = ips.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetworkV6);

        ipToReturn ??= ips.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);

        if (ipToReturn is null)
            throw new ArgumentException($"DNS {dns} could not be resolved to neither an IPv6 nor an IPv4 Address.");

        return ipToReturn;
    }

    /// <summary>
    /// Creates a new config file, filling it out with default values.
    /// </summary>
    public static void CreateNewConfig()
    {
        using StreamWriter sw = new StreamWriter(GetConfigFilePath());
        sw.WriteLine("#mode: Set to server if you want to host a local server over UDP, client if you want to connect to a server over UDP.");
        sw.WriteLine($"{Mode}={(NodeOptions.IsServer ? Server : Client)}");
        sw.WriteLine();
        sw.WriteLine("#endpoint, servers: The IP address of the TCP server to connect to for forwarding over UDP. Client: The IP address of the UDP server to connect to.");
        sw.WriteLine($"{Endpoint}={NodeOptions.Endpoint.Address}");
        sw.WriteLine();
        sw.WriteLine("#mediationIP: The public IP and port of the mediation server you want to connect to.");
        sw.WriteLine($"{MediationIp}={NodeOptions.MediationIp}");
        sw.WriteLine();
        sw.WriteLine("#remoteIP, clients: The public IP of the peer you want to connect to.");
        sw.WriteLine($"{RemoteIp}={NodeOptions.RemoteIp}");
        sw.WriteLine();
        sw.WriteLine("#localPort: servers: The UDP server port. client: The TCP port to host the forwarded server on.");
        sw.WriteLine($"{LocalPort}={NodeOptions.LocalPort}");
        sw.WriteLine();
        sw.WriteLine("#mediationClientPort: The UDP mediation client port. This is the port that will have a hole punched through the NAT by the mediation server, and all traffic will pass through it.");
        sw.WriteLine($"{MediationClientPort}={NodeOptions.MediationClientPort}");
        sw.WriteLine();
        sw.WriteLine("#uploadSpeed/downloadSpeed: Specify your connection limit (kB/s), this program sends at a fixed rate.");
        sw.WriteLine($"{UploadSpeed}={NodeOptions.UploadSpeed}");
        sw.WriteLine($"{DownloadSpeed}={NodeOptions.DownloadSpeed}");
        sw.WriteLine();
        sw.WriteLine("#minRetransmitTime: How many milliseconds delay to send unacknowledged packets.");
        sw.WriteLine($"{MinRetransmitTime}={NodeOptions.MinRetransmitTime}");
        sw.WriteLine();
        sw.WriteLine($"#connectionType: The protocol that the end application will be using ({TCP} or {UDP}).");
        sw.WriteLine($"{ConnectionType}={NodeOptions.ConnectionType}");
    }


    /// <summary>
    /// Prompts in the console to create a new config file.
    /// </summary>
    /// <param name="exitOnExistingConfig">Whether to exit if the config file already exists, instead of erroring.</param>
    /// <returns><see langword="true"/> if a config file was created or
    /// the file exists and <paramref name="exitOnExistingConfig"/> is <see langword="true"/>.
    /// <see langword="false"/> if the user quit out.</returns>
    public static bool CreateNewConfigPrompt(bool exitOnExistingConfig = true)
    {
        bool doesFileExist = File.Exists(GetConfigFilePath());
        if (doesFileExist && exitOnExistingConfig)
            return true;

        if (!doesFileExist)
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
                NodeOptions.IsServer = false;
                NodeOptions.LocalPort = 5001;
                CreateNewConfig();
                return true;
            }
            case 's':
            {
                NodeOptions.IsServer = true;
                NodeOptions.Endpoint = new IPEndPoint(IPAddress.Loopback, 25565);
                NodeOptions.LocalPort = 5001;
                CreateNewConfig();
                return true;
            }
            default:
            {
                Console.WriteLine("Quitting...");
                return false;
            }
        }
    }

    /// <summary>
    /// Resolves a string as either IP address or hostname, and assigns it to <see cref="NodeOptions.Endpoint"/>.
    /// </summary>
    /// <param name="endpoint">The endpoint to resolve.</param>
    /// <param name="port">The port to use for the endpoints.</param>
    /// <returns><see langword="true"/> if it can be successfully resolved, otherwise <see langword="false"/>.</returns>
    private static bool ResolveAddressAndAssignToEndpoints(string endpoint, int port)
    {
        if (endpoint.Contains(':'))
        {
            Console.Error.WriteLine($"Port cannot be specified for {Endpoint}!");
            return false;
        }

        try
        {
            NodeOptions.Endpoint = new IPEndPoint(GetIPFromDnsResolve(endpoint), port);
        }
        catch
        {
            Console.Error.WriteLine("Could not resolve Endpoint.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// The file path to where the config.txt for NATTunnel is located, depending on the OS.
    /// </summary>
    /// <returns>The file path to where the config.txt is located for a known OS, <see langword="null"/> for an unknown OS.</returns>
    public static string GetConfigFilePath()
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            string natTunnelDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/NATTunnel";
            Directory.CreateDirectory(natTunnelDir);
            return natTunnelDir + "/config.txt";
        }

        // Special case for macos, because the applicationData folder is currently bugged on macos+.net
        if (OperatingSystem.IsMacOS())
        {
            string natTunnelDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Library/Application Support/NATTunnel";
            Directory.CreateDirectory(natTunnelDir);
            return natTunnelDir + "/config.txt";
        }

        return null;
    }
}