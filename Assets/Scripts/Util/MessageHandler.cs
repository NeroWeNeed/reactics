using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Reactics.Util
{
    public delegate void MessageListener<TMessage>(TMessage message);

    public delegate void MessageListenerWithCallback<TMessageIn, TMessageOut>(TMessageIn message, MessageListener<TMessageOut> callback);
    public class MessageHandler<TSuper>
    {
        private readonly Dictionary<Type, ICollection<dynamic>> listeners = new Dictionary<Type, ICollection<dynamic>>();

        public void Subscribe<TMessage>(MessageListener<TMessage> listener) where TMessage : TSuper
        {
            Type type = typeof(TMessage);
            if (!listeners.ContainsKey(type))
            {
                listeners[type] = new LinkedList<dynamic>();
            }
            listeners[type].Add(listener);
        }
        public void Subscribe<TMessage>(MessageListenerWithCallback<TMessage, TSuper> listener) where TMessage : TSuper
        {
            Type type = typeof(TMessage);
            if (!listeners.ContainsKey(type))
            {
                listeners[type] = new LinkedList<dynamic>();
            }
            listeners[type].Add(listener);
        }

        public void Publish<TMessage>(TMessage message, MessageListener<TSuper> callback) where TMessage : TSuper
        {
            this.listeners.TryGetValue(message.GetType(), out ICollection<dynamic> listeners);
            if (listeners == null)
                return;
            Type typeWithCallback = typeof(MessageListenerWithCallback<,>).MakeGenericType(message.GetType(), typeof(TSuper));
            Type typeWithoutCallback = typeof(MessageListener<>).MakeGenericType(message.GetType());
            foreach (var item in listeners)
            {
                if (typeWithoutCallback.IsAssignableFrom(item.GetType()))
                {
                    ((Delegate)item).DynamicInvoke(message);
                }
                else if (typeWithCallback.IsAssignableFrom(item.GetType()) && callback != null)
                {
                    ((Delegate)item).DynamicInvoke(message, callback);
                }
            }
        }
        public void SubscribeFromInstance(object instance)
        {
            Type type;
            int parameterLength;
            Delegate del;
            foreach (var method in instance.GetType().GetMethods())
            {

                if (method.GetCustomAttribute(typeof(MessageSubscriber)) == null)
                    continue;
                parameterLength = method.GetParameters().Length;
                if (parameterLength <= 0 || parameterLength > 2)
                    continue;

                type = method.GetParameters()[0].ParameterType;

                if (!typeof(TSuper).IsAssignableFrom(type))
                    continue;

                if (parameterLength == 1)
                {
                    del = method.CreateDelegate(typeof(MessageListener<>).MakeGenericType(type), instance);
                }
                else
                {
                    del = method.CreateDelegate(typeof(MessageListenerWithCallback<,>).MakeGenericType(type, typeof(TSuper)), instance);
                }
                if (!listeners.ContainsKey(type))
                {
                    listeners[type] = new LinkedList<dynamic>();
                }
                listeners[type].Add(del);

            }
        }
    }
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageSubscriber : Attribute
    {
    }

}