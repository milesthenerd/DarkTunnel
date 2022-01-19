using System;

namespace NATTunnel.Common
{
    public class MessageTypeAttribute : Attribute
    {
        public MessageType type;
        public MessageTypeAttribute(MessageType type)
        {
            this.type = type;
        }
    }
}
