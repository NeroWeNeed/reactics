using Reactics.Battle;
using Reactics.Battle.Packets;
using UnityEngine;
using System.Linq;
using Reactics.Util;

namespace Reactics.Test
{

    [RequireComponent(typeof(ClientComponent))]
    public class TestClient : MonoBehaviour
    {
        private ClientComponent client;
        private TestPacketRouter packetRouter;
        private void Awake()
        {
            client = GetComponent<ClientComponent>();
            packetRouter = new TestPacketRouter(name);
        }
        private void Start()
        {
            client.AddListener(x => packetRouter.Route(x, null));
            client.Push(new JoinRequestPacket(name, true));
        }

        public class TestPacketRouter
        {
            private MessageHandler<Packet> handler;
            private string name;

            public TestPacketRouter(string name)
            {
                this.name = name;
                handler = new MessageHandler<Packet>();
                handler.SubscribeFromInstance(this);
            }

            public void Route(Packet packet, MessageListener<Packet> callback)
            {
                handler.Publish(packet, callback);
            }
            [MessageSubscriber]
            public void HandleJoinResponse(JoinResponsePacket packet)
            {
                Debug.Log($"{name} Joined as {(packet.IsPlayer ? "a Player" : "an Observer")}");
            }
            [MessageSubscriber]
            public void HandlePlayerListResponse(PlayerListResponsePacket packet)
            {
                Debug.Log($"Current Players: {(packet.profiles.Select(x => x.Name).Aggregate((x, y) => x + ' ' + y))}");
            }
        }
    }
}