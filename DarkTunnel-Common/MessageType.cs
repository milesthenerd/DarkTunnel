namespace DarkTunnel.Common
{
    public enum MessageType
    {
        //HEARTBEAT is not used, ACKs do the job of keeping the UDP connection alive
        HEARTBEAT = 0,
        DISCONNECT = 1,
        NEWCONNECTIONREQUEST = 10,
        NEWCONNECTIONREPLY = 11,
        PINGREQUEST = 20,
        PINGREPLY = 21,
        DATA = 30,
        ACK = 31,
        MASTERSERVERINFOREQUST = 100,
        MASTERSERVERINFOREPLY = 101,
        MASTERSERVERPUBLISHREQUEST = 110,
        MASTERSERVERPUBLISHREPLY = 111,
        MASTER_PRINT_CONSOLE = 120
    }
}