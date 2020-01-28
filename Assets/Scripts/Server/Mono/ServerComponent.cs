using Reactics.Battle.Packets;
using Reactics.Battle.Servers;
using Reactics.Util;
using UnityEngine;

namespace Reactics.Battle
{
    public class ServerComponent : MonoBehaviour
    {
        private readonly Server server = new Server(typeof(RootServer));
        public void Publish(Packet packet,MessageListener<Packet> callback)
        {
            server.Publish(packet,callback);
        }
    }
}