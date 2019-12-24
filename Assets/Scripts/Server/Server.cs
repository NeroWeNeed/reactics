using System;
using System.Collections.Generic;

namespace Reactics.Battle
{

    public class Server : PacketManager
    {


        public ServerStatus Status { get; private set; }
        public int MaxPlayers { get; private set; }
        public int CurrentPlayers { get; private set; }

        private List<ServerPlayer> players;

        private IConnectionFactory connectionFactory;

        private List<IConnection> connections;

        public Server(IConnectionFactory connectionFactory, int maxPlayers = 2) : base()
        {
            this.MaxPlayers = maxPlayers;
            this.connectionFactory = connectionFactory;
            Status = ServerStatus.ACCEPTING_PLAYERS;
            players = new List<ServerPlayer>();
            connections = new List<IConnection>();
        }

        public IConnection Connect(connectionHandler handler)
        {
            IConnection connection = connectionFactory.Create(this, handler);
            connections.Add(connection);
            return connection;
        }



        [PacketHandler(typeof(JoinPacket))]
        public void ProcessJoin(PacketSenderDelegate packetSender, JoinPacket packet)
        {
            ServerPlayer player;
            if (packet.PlayerType == ServerPlayerType.PLAYER)
            {
                if (CurrentPlayers < MaxPlayers && Status == ServerStatus.ACCEPTING_PLAYERS)
                {
                    CurrentPlayers++;
                    player = new ServerPlayer(ServerPlayerType.PLAYER);
                }
                else
                {
                    player = new ServerPlayer(ServerPlayerType.OBSERVER);
                }

            }
            else
            {
                player = new ServerPlayer(ServerPlayerType.OBSERVER);
            }
            players.Add(player);
            packetSender.Invoke(new JoinResponsePacket(player.Type, player.Id));
        }


    }





    class ServerPlayer
    {
        public Guid Id { get; private set; }
        public ServerPlayerType Type { get; private set; }
        public ServerPlayer(ServerPlayerType type)
        {
            Id = System.Guid.NewGuid();
            Type = type;
        }

    }
    public enum ServerPlayerType
    {
        PLAYER, OBSERVER
    }
    public enum ServerStatus
    {
        ACCEPTING_PLAYERS, PLAYING, FINISHED
    }


}
