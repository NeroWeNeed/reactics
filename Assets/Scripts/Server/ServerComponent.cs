using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reactics.Battle
{
    public class BattleServerComponent : MonoBehaviour
    {

        private Server server;
        private IConnection client;
        private List<Client> clients = new List<Client>();

        private void Start()
        {
            server = new Server(new DirectConnectionFactory());
            client = server.Connect(x =>
            {
                Debug.Log(x);
            });
            client.Send(new JoinPacket(ServerPlayerType.PLAYER));

        }
        private bool pressed = false;




    }

}
