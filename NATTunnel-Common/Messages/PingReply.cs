using System;
using System.IO;

namespace NATTunnel.Common.Messages
{
    [MessageTypeAttribute(MessageType.PING_REPLY)]
    public class PingReply : INodeMessage
    {
        public int id;
        public long sendTime;
        public string ep;

        public PingReply()
        {
            id = 0;
            sendTime = 0;
            ep = "";
        }

        public PingReply(int id, long sendTime, string ep)
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