using System.IO;

namespace NATTunnel.Common
{
    public interface IMessage
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}
