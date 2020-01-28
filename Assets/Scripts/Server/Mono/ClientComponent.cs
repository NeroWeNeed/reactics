using Reactics.Battle.Events;
using Reactics.Battle.Packets;
using UnityEngine;
using UnityEngine.Events;

namespace Reactics.Battle
{
    public class ClientComponent : MonoBehaviour
    {
        [SerializeField]
        private ServerComponent server;
        public ServerComponent Server { get => server; }
        [SerializeField]
        private PacketReceivedEvent packetReceivedEvent;

        private void Awake()
        {
            if (server == null)
            {
                server = GetComponentInParent<ServerComponent>();
                if (server == null)
                    throw new UnityException("Server Component not set or found.");

            }
            if (packetReceivedEvent == null)
                packetReceivedEvent = new PacketReceivedEvent();
        }
        public void AddListener(UnityAction<Packet> listener)
        {

            packetReceivedEvent.AddListener(listener);
        }
        public void RemoveListener(UnityAction<Packet> listener)
        {
            packetReceivedEvent.RemoveListener(listener);
        }
        public void RemoveAllListeners()
        {
            packetReceivedEvent.RemoveAllListeners();
        }
        public void Push(Packet packet)
        {
            Server.Publish(packet, receivedPacket =>
            {
                packetReceivedEvent.Invoke(receivedPacket);
            });
        }
    }
}