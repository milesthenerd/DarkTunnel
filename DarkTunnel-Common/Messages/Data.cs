using System.IO;

namespace DarkTunnel.Common.Messages
{
    [MessageTypeAttribute(MessageType.DATA)]
    public class Data : INodeMessage
    {
        public int id;
        public long streamPos;
        public long streamAck;
        public byte[] tcpData;
        public string ep;

        public Data(int id, long streamPos, long streamAck, byte[] tcpData, string ep)
        {
            this.id = id;
            this.streamPos = streamPos;
            this.streamAck = streamAck;
            this.tcpData = tcpData;
            this.ep = ep;
        }

        public int GetID()
        {
            return id;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(id);
            writer.Write(streamPos);
            writer.Write(streamAck);
            writer.Write((short)tcpData.Length);
            writer.Write(tcpData);
            writer.Write(ep);
        }
        public void Deserialize(BinaryReader reader)
        {
            id = reader.ReadInt32();
            streamPos = reader.ReadInt64();
            streamAck = reader.ReadInt64();
            int length = reader.ReadInt16();
            tcpData = reader.ReadBytes(length);
            ep = reader.ReadString();
        }
    }
}
