using System;
using System.Collections.Generic;
using Reactics.Battle.Packets;
using System.Linq;
using Reactics.Util;


namespace Reactics.Battle
{
    public class Server
    {
        private readonly MessageHandler<Packet> messageHandler = new MessageHandler<Packet>();
        private readonly List<object> fragments = new List<object>();
        public Server(ServerFactory factory, params Type[] views)
        {
            Init(factory.Invoke, views);
        }
        public Server(params Type[] views)
        {
            DefaultServerFactory factory = new DefaultServerFactory(messageHandler);
            Init(factory.Invoke, views);
        }
        private void Init(ServerFactory factory, params Type[] views)
        {
            object t;
            ServerView[] viewAttributes;
            foreach (var type in views.Where(x => !this.fragments.Any(y => y.GetType().Equals(x))).Distinct())
            {
                viewAttributes = (ServerView[])type.GetCustomAttributes(typeof(ServerView), true);

                t = factory.Invoke(type);
                this.fragments.Add(t);

                messageHandler.SubscribeFromInstance(t);
                if (viewAttributes != null && viewAttributes.Length > 0)
                {
                    Init(factory, viewAttributes.SelectMany(x => x.types).Where(x => !this.fragments.Any(y => y.GetType().Equals(x))).ToArray());
                }
            }
        }
        public void Publish<T>(T packet, MessageListener<Packet> callback) where T : Packet
        {
            messageHandler.Publish(packet, callback);
        }
    }
    class DefaultServerFactory
    {
        private readonly InjectActivator injectActivator = new InjectActivator();
        public DefaultServerFactory(MessageHandler<Packet> handler)
        {
            injectActivator.Inject(handler);
        }
        public object Invoke(Type type)
        {
            return injectActivator.CreateInstance(type);
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class ServerView : Attribute
    {
        public readonly Type[] types;
        public ServerView(params Type[] types)
        {
            this.types = types;
        }
    }
    public delegate object ServerFactory(Type type);
}