



using UnityEngine;

namespace Reactics.Battle
{
    public class Client : PacketRouter
    {
        public Client() : base()
        {
            Debug.Log(handlerMap.Count);
            foreach (var item in this.handlerMap.Keys)
            {
                Debug.Log(item);
            }
            
        }

        [PacketRoute(typeof(JoinResponsePacket))]
        public void ProcessJoinResponsePacket(PacketSenderDelegate packetSender, JoinResponsePacket packet)
        {
            Debug.Log($"Joined as {packet.PlayerType} with id {packet.Id} ");
        }
    }
}