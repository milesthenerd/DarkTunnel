using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NATTunnel.Common
{
    public class UdpConnection
    {
        private bool running = true;
        private Socket udp;
        private Thread recvThread;
        private Thread sendThread;
        private AutoResetEvent are = new AutoResetEvent(false);
        private Action<IMessage, IPEndPoint> receiveCallback;
        private ConcurrentQueue<Tuple<IMessage, IPEndPoint>> sendMessages = new ConcurrentQueue<Tuple<IMessage, IPEndPoint>>();

        public UdpConnection(Socket udp, Action<IMessage, IPEndPoint> receiveCallback)
        {
            this.udp = udp;
            this.receiveCallback = receiveCallback;
            recvThread = new Thread(ReceiveLoop) { Name = "UdpConnection-Receive" };
            recvThread.Start();
            sendThread = new Thread(SendLoop) { Name = "UdpConnection-Send" };
            sendThread.Start();
        }

        public void Stop()
        {
            running = false;
            recvThread.Join();
            sendThread.Join();
        }

        private void ReceiveLoop()
        {
            byte[] recvBuffer = new byte[1500];
            while (running)
            {
                if (!udp.Poll(5000, SelectMode.SelectRead))
                    continue;

                int receivedBytes = 0;
                EndPoint recvEndpoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                try
                {
                    receivedBytes = udp.ReceiveFrom(recvBuffer, ref recvEndpoint);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving: {e}");
                    continue;
                }
                using MemoryStream ms = new MemoryStream(recvBuffer, 0, receivedBytes, false);
                using BinaryReader br = new BinaryReader(ms);
                IMessage receivedMessage = Header.DeframeMessage(br);
                receiveCallback(receivedMessage, (IPEndPoint)recvEndpoint);
            }
        }

        private void SendLoop()
        {
            while (running)
            {
                are.WaitOne(100);
                while (sendMessages.TryDequeue(out Tuple<IMessage, IPEndPoint> sendMessage))
                {
                    byte[] sendBytes = Header.FrameMessage(sendMessage.Item1);
                    int sendSize = 8 + BitConverter.ToInt16(sendBytes, 6);
                    try
                    {
                        udp.SendTo(sendBytes, 0, sendSize, SocketFlags.None, sendMessage.Item2);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error sending: {e}");
                    }
                }
            }
        }

        public void Send(IMessage message, IPEndPoint endpoint)
        {
            sendMessages.Enqueue(new Tuple<IMessage, IPEndPoint>(message, endpoint));
            are.Set();
        }
    }
}