

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reactics.Battle
{

    public interface IPacket
    {
        long Timestamp { get; }
    }
    public struct JoinPacket : IPacket
    {

        readonly public ServerPlayerType PlayerType;
        readonly public long timestamp;

        public long Timestamp => timestamp;



        public JoinPacket(ServerPlayerType playerType)
        {
            timestamp = DateTime.Now.ToFileTimeUtc();
            PlayerType = playerType;
        }

    }
    public struct JoinResponsePacket : IPacket
    {
        readonly public ServerPlayerType PlayerType;

        readonly public Guid Id;

        readonly public long timestamp;

        public long Timestamp => timestamp;

        public JoinResponsePacket(ServerPlayerType playerType, Guid id)
        {
            timestamp = DateTime.Now.ToFileTimeUtc();
            PlayerType = playerType;
            Id = id;
        }
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class PacketHandler : Attribute
    {
        public Type PacketType { get; private set; }

        public PacketHandler(Type packetType)
        {
            this.PacketType = packetType;
        }
    }

    public abstract class PacketManager
    {
        public delegate void PacketHandlerDelegate(PacketSenderDelegate packetSender, IPacket packet);

        protected Dictionary<Type, PacketHandlerDelegate> handlerMap = new Dictionary<Type, PacketHandlerDelegate>();
        public PacketManager()
        {

            PacketHandler handlerAttr;

            foreach (var item in this.GetType().GetMethods())
            {
                handlerAttr = (PacketHandler)Attribute.GetCustomAttribute(item, typeof(PacketHandler));
                if (handlerAttr != null)
                {

                    handlerMap[handlerAttr.PacketType] = (x, y) =>
                    {
                        item.Invoke(this, new object[] { x, y });
                    };

                }

            }
            Debug.Log($"{handlerMap.Count} handlers");
        }

        public void Process(PacketSenderDelegate packetSender, IPacket packet)
        {
            if (handlerMap.ContainsKey(packet.GetType()))
            {
                handlerMap[packet.GetType()].Invoke(packetSender, packet);
            }
            else
                throw new ArgumentException("Unknown Packet Provided");
        }
    }




}