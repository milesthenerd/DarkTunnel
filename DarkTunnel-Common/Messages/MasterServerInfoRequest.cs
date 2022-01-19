using System.IO;

namespace NATTunnel.Common.Messages
{
    [MessageTypeAttribute(MessageType.MASTER_SERVER_INFO_REQUEST)]
    public class MasterServerInfoRequest : IMessage
    {
        public int server;
        public int client;

        public MasterServerInfoRequest()
        {
            server = 0;
            client = 0;
        }

        public MasterServerInfoRequest(int server = 0, int client = 0)
        {
            this.server = server;
            this.client = client;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(server);
            writer.Write(client);
        }
        public void Deserialize(BinaryReader reader)
        {
            server = reader.ReadInt32();
            client = reader.ReadInt32();
        }
    }
}