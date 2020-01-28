using System;

namespace Reactics.Battle.Packets
{
    [Serializable]
    public abstract class Packet
    {
        readonly long Timestamp;
    }

    public class ErrorPacket : Packet
    {
        public readonly string Message;

    }

    public class InvalidPacket : Packet
    {
        private readonly Packet Packet;

        public InvalidPacket(Packet packet)
        {
            Packet = packet;
        }
    }

}