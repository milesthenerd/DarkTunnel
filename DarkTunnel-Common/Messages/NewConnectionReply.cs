using System.IO;

namespace DarkTunnel.Common.Messages
{
    [MessageTypeAttribute(MessageType.NEW_CONNECTION_REPLY)]
    public class NewConnectionReply : INodeMessage
    {
        public int id;
        public int protocol_version;
        public int downloadRate;
        public string ep;

        public NewConnectionReply()
        {
            id = 0;
            protocol_version = 0;
            downloadRate = 0;
            ep = "";
        }

        public NewConnectionReply(int id, int protocol_version, int downloadRate, string ep = "")
        {
            this.id = id;
            this.protocol_version = protocol_version;
            this.downloadRate = downloadRate;
            this.ep = ep;
        }

        public int GetID()
        {
            return id;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(id);
            writer.Write(protocol_version);
            writer.Write(downloadRate);
            writer.Write(ep);
        }
        public void Deserialize(BinaryReader reader)
        {
            id = reader.ReadInt32();
            protocol_version = reader.ReadInt32();
            downloadRate = reader.ReadInt32();
            ep = reader.ReadString();
        }
    }
}