using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public static class PortUtility {
        public const string NotificationClassName = "notification";
        public static void MakeObservable(this Port port) {
            Action<Port> onConnect;
            Action<Port> onDisconnect;
            var onConnectField = port.GetType().GetField("OnConnect", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var onDisconnectField = port.GetType().GetField("OnDisconnect", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            onConnect = (Action<Port>)Delegate.Remove((Action<Port>)onConnectField.GetValue(port), new Action<Port>(ConnectAction));
            onDisconnect = (Action<Port>)Delegate.Remove((Action<Port>)onDisconnectField.GetValue(port), new Action<Port>(DisconnectAction));
            onConnect = (Action<Port>)Delegate.Combine(onConnect, new Action<Port>(ConnectAction));
            onDisconnect = (Action<Port>)Delegate.Combine(onDisconnect, new Action<Port>(DisconnectAction));
            onConnectField.SetValue(port, onConnect);
            onDisconnectField.SetValue(port, onDisconnect);
        }
        private static void ConnectAction(Port port) {
            using (PortChangedEvent evt = PortChangedEvent.GetPooled(port.connections)) {
                evt.target = port;
                port.SendEvent(evt);
            }
        }
        private static void DisconnectAction(Port port) {
            using (PortChangedEvent evt = PortChangedEvent.GetPooled(port.connections)) {
                evt.target = port;
                port.SendEvent(evt);
            }
        }
        public static void ErrorNotification(this Port port, string message) {
            if (port.node != null) {
                port.ClearNotifications();
                var badge = IconBadge.CreateError(message);
                badge.AddToClassList(NotificationClassName);
                port.node.Add(badge);
                badge.AttachTo(port, SpriteAlignment.LeftCenter);
            }

        }
        public static void Notification(this Port port, string message) {
            if (port.node != null) {
                port.ClearNotifications();
                var badge = IconBadge.CreateComment(message);
                badge.AddToClassList(NotificationClassName);
                port.node.Add(badge);
                badge.AttachTo(port, SpriteAlignment.LeftCenter);
            }
        }
        public static void ClearNotifications(this Port port) {
            if (port.node != null) {
                port.node.Query<VisualElement>(null, NotificationClassName).ForEach((element) => element.RemoveFromHierarchy());
            }
        }

    }


}