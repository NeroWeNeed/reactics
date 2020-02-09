using System;
using System.Linq;
using Reactics.Battle.Packets;
using Reactics.Util;

namespace Reactics.Battle.Servers
{
    /* 
    TODO: Hand Admin Powers over to another client
    TODO: Handle when admin leaves

     */
    public class LobbyServerView
    {
        private readonly Configuration configuration;
        private readonly MessageHandler<Packet> handler;
        private readonly Status status;
        private readonly object _playerLock = new object();

        [Inject]
        public LobbyServerView(Configuration configuration, Status status, MessageHandler<Packet> handler)
        {
            this.configuration = configuration;
            this.handler = handler;
            this.status = status;
        }
        public void Route(Packet packet, MessageListener<Packet> callback)
        {
            handler.Publish(packet, callback);
        }
        [MessageSubscriber]
        public void HandleJoin(JoinRequestPacket packet, MessageListener<Packet> callback)
        {
            if (status.Add(packet.Name, packet.IsPlayer, configuration, callback, out ClientInfo clientInfo, out PrivateKey privateKey))
            {
                callback.Invoke(new JoinResponsePacket(privateKey, clientInfo.Profile.IsPlayer));
                PushPlayerList();
            }
        }
        [MessageSubscriber]
        public void HandleLeave(LeaveRequestPacket packet)
        {
            if (status.Remove(packet.PrivateKey, out ClientInfo clientInfo))
            {
                clientInfo.Callback.Invoke(new LeaveResponsePacket());
                PushPlayerList();
            }
        }
        private void PushPlayerList()
        {
            PlayerListResponsePacket responsePacket = new PlayerListResponsePacket(status.ClientInfo.Select(x => x.Profile).ToList().AsReadOnly());
            foreach (var item in status.ClientInfo)
            {
                item.Callback.Invoke(responsePacket);
            }
        }
    }
    public class Status
    {
        public readonly GuidStore<ClientInfo> ClientInfo;
        public int PlayerCount { get; private set; }

        public int ClientCount { get; private set; }

        private readonly object _playerLock = new object();
        public Status()
        {
            ClientInfo = new GuidStore<ClientInfo>();
        }
        public bool Add(string name, bool asPlayer, Configuration configuration, MessageListener<Packet> callback, out ClientInfo output, out PrivateKey outputKey)
        {
            ClientInfo clientInfo;
            PlayerProfile profile;
            PrivateKey privateKey;
            PublicKey publicKey;
            if (ClientCount + 1 >= configuration.MaxClients)
            {
                output = default;
                outputKey = default;
                return false;
            }

            lock (_playerLock)
            {
                if (ClientCount + 1 >= configuration.MaxClients)
                {
                    output = default;
                    outputKey = default;
                    return false;
                }
                publicKey = PublicKey.Create();
                if (asPlayer && PlayerCount < configuration.MaxPlayers)
                {
                    PlayerCount++;
                    profile = new PlayerProfile(name, true,ClientCount == 0, publicKey);
                }
                else
                {
                    profile = new PlayerProfile(name, false,ClientCount == 0, publicKey);
                }
                ClientCount++;
                clientInfo = new ClientInfo(profile, callback);
                privateKey = ClientInfo.Add(clientInfo, publicKey);
            }
            output = clientInfo;
            outputKey = privateKey;
            return true;
        }

        public bool Remove(PrivateKey key, out ClientInfo clientInfo)
        {
            lock (_playerLock)
            {
                if (ClientInfo.Contains(key))
                {

                    clientInfo = ClientInfo[key];
                    ClientInfo.Remove(key);
                    if (clientInfo.Profile.IsPlayer)
                        PlayerCount--;
                    ClientCount--;
                    clientInfo.Callback.Invoke(new LeaveResponsePacket());
                    return true;
                }
            }
            clientInfo = default;
            return false;
        }
    }
    public struct ClientInfo
    {

        public readonly PlayerProfile Profile;

        public readonly MessageListener<Packet> Callback;

        public readonly PublicKey Id;

        public ClientInfo(PlayerProfile profile, MessageListener<Packet> callback)
        {
            Profile = profile;
            Callback = callback;

        }
    }
    public struct PlayerProfile
    {
        public readonly string Name;

        public readonly bool IsPlayer;

        public readonly bool IsAdmin;

        public readonly PublicKey Id;

        public PlayerProfile(string name, bool isPlayer,bool isAdmin, PublicKey publicKey)
        {
            Name = name;
            IsPlayer = isPlayer;
            IsAdmin = isAdmin;
            Id = publicKey;
        }
    }
    public class Configuration
    {
        public MapAsset Map { get; set; }

        public int MaxPlayers { get; set; }

        public int MaxClients { get; set; }

        public int MaxUnits { get; set; }

        public Configuration() : this(null, 2, 8, 6) { }
        public Configuration(MapAsset map = null, int maxPlayers = 2, int maxClients = 8, int maxUnits = 6)
        {
            Map = map;
            MaxPlayers = maxPlayers;
            MaxClients = maxClients;
            MaxUnits = maxUnits;
        }
    }
}