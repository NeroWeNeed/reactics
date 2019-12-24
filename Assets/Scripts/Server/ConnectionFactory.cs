

namespace Reactics.Battle
{
    public delegate void connectionHandler(IPacket packet);

    public interface IConnectionFactory
    {
        IConnection Create(Server server, connectionHandler handler);
    }

    public interface IConnection
    {
        void Send(IPacket packet);

        void Receive(IPacket packet);
    }

    public class DirectConnectionFactory : IConnectionFactory
    {
        public IConnection Create(Server server, connectionHandler handler)
        {
            return new DirectConnection(server, handler);
        }
    }

    public class DirectConnection : IConnection
    {

        private Server server;
        private connectionHandler handler;

        public DirectConnection(Server server, connectionHandler handler)
        {
            this.server = server;
            this.handler = handler;
        }

        public void Receive(IPacket packet)
        {
            handler.Invoke(packet);
        }

        public void Send(IPacket packet)
        {
            server.Process(Receive, packet);
        }
    }

}