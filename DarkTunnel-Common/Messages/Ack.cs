using System.IO;

namespace NATTunnel.Common.Messages
{
    [MessageTypeAttribute(MessageType.ACK)]
    public class Ack : INodeMessage
    {
        public int id;
        public long streamAck;
        public string ep;

        public Ack()
        {
            id = 0;
            streamAck = 0;
            ep = "";
        }

        public Ack(int id, long streamAck, string ep)
        {
            this.id = id;
            this.streamAck = streamAck;
            this.ep = ep;
        }

        public int GetID()
        {
            return id;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(id);
            writer.Write(streamAck);
            writer.Write(ep);
        }
        public void Deserialize(BinaryReader reader)
        {
            id = reader.ReadInt32();
            streamAck = reader.ReadInt64();
            ep = reader.ReadString();
        }
    }
}