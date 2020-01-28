using UnityEngine.Events;
using Reactics.Battle.Packets;

namespace Reactics.Battle.Events
{
    public class PacketReceivedEvent : UnityEvent<Packet> { }
}