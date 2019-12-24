

namespace Reactics.Battle
{
    public delegate void PacketSenderDelegate(IPacket packet);


    public interface IBattleTransmission
    {

        void ToServer(IPacket packet);

        void ToClient(IPacket packet);
    }
    public class DirectBattleTransmission : IBattleTransmission
    {
        public Client Client { get; private set; }
        public Server Server { get; private set; }

        private readonly PacketSenderDelegate ToServerPacketSender;

        private readonly PacketSenderDelegate ToClientPacketSender;

        public DirectBattleTransmission(Server server, Client client)
        {
            Server = server;
            Client = client;
            ToServerPacketSender = ToServer;
            ToClientPacketSender = ToClient;
        }

        public void ToClient(IPacket packet)
        {
            Client.Process(ToServerPacketSender, packet);
        }
        public void ToServer(IPacket packet)
        {
            Server.Process(ToClientPacketSender, packet);

        }
    }
}