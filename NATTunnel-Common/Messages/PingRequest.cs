using System.IO;

namespace NATTunnel.Common.Messages
{
    [MessageTypeAttribute(MessageType.PING_REQUEST)]
    public class PingRequest : INodeMessage
    {
        public int id;
        public long sendTime;
        public string ep;

        public PingRequest()
        {
            id = 0;
            sendTime = 0;
            ep = "";
        }

        public PingRequest(int id, long sendTime, string ep)
        {
            this.id = id;
            this.sendTime = sendTime;
            this.ep = ep;
        }

        public int GetID()
        {
            return id;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(id);
            writer.Write(sendTime);
            writer.Write(ep);
        }
        public void Deserialize(BinaryReader reader)
        {
            id = reader.ReadInt32();
            sendTime = reader.ReadInt64();
            ep = reader.ReadString();
        }
    }
}