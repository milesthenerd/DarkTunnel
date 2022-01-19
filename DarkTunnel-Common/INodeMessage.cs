namespace DarkTunnel.Common
{
    public interface INodeMessage : IMessage
    {
        //TODO: why require this method, if all implementations just use a public id anyway!?
        int GetID();
    }
}