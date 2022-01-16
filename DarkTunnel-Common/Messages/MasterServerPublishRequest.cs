using System.IO;

namespace DarkTunnel.Common.Messages
{
    [MessageTypeAttribute(MessageType.MASTER_SERVER_PUBLISH_REQUEST)]
    public class MasterServerPublishRequest : IMessage
    {
        public int id;
        public int secret;
        public int localPort;

        public MasterServerPublishRequest(int id, int secret, int localPort)
        {
            this.id = id;
            this.secret = secret;
            this.localPort = localPort;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(id);
            writer.Write(secret);
            writer.Write(localPort);
        }
        public void Deserialize(BinaryReader reader)
        {
            id = reader.ReadInt32();
            secret = reader.ReadInt32();
            localPort = reader.ReadInt32();
        }
    }
}
