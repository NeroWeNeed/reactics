using System;
using System.Collections.Generic;
using Reactics.Battle;
using Reactics.Battle.Packets;
using Reactics.Battle.Servers;
using Reactics.Util;

namespace Reactics.Battle
{

    public abstract class SelfUpdatePacket : Packet
    {
        public readonly PrivateKey PrivateKey;
    }
    public class JoinRequestPacket : Packet
    {
        public readonly string Name;
        public readonly bool IsPlayer;
        public JoinRequestPacket(string name, bool isPlayer)
        {
            Name = name;
            IsPlayer = isPlayer;
        }
    }
    public class JoinResponsePacket : Packet
    {
        public readonly bool IsPlayer;
        public readonly PrivateKey Key;

        public bool IsObserver { get => !IsPlayer; }
        public JoinResponsePacket(PrivateKey key, bool isPlayer)
        {
            Key = key;
            IsPlayer = isPlayer;
        }
    }
    public class LeaveRequestPacket : SelfUpdatePacket
    {
    }
    public class LeaveResponsePacket : Packet
    {
    }

    public class LobbyStatusRequestPacket : Packet
    {

    }

    public class PlayerListResponsePacket : Packet
    {
        public readonly IReadOnlyCollection<PlayerProfile> profiles;

        public PlayerListResponsePacket(IReadOnlyCollection<PlayerProfile> profiles)
        {
            this.profiles = profiles;
        }
    }



}