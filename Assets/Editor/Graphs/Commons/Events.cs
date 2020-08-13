using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {
    public class ChangeFieldEvent : EventBase<ChangeFieldEvent> {
        public Type type { get; protected set; }

        public string name { get; protected set; }

        public object value { get; protected set; }
        public ChangeFieldEvent() {
            base.Init();
            Initialize();
        }
        private void Initialize() {
            var prop = typeof(ChangeFieldEvent).GetProperty("propagation", BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.Public);
            prop?.SetValue(this, 3);
        }
        public static ChangeFieldEvent GetPooled(Type type, string name, object value) {
            ChangeFieldEvent e = GetPooled();
            e.type = type;
            e.name = name;
            e.value = value;
            e.Initialize();

            return e;
        }
    }
    public class PortChangedEvent : EventBase<PortChangedEvent> {

        public IEnumerable<Edge> edges { get; private set; }
        public PortChangedEvent() {
            base.Init();
            Initialize();
        }
        private void Initialize() {
            var prop = typeof(PortChangedEvent).GetProperty("propagation", BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.Public);
            prop?.SetValue(this, 3);
        }
        public static PortChangedEvent GetPooled(IEnumerable<Edge> edges) {
            var evt = GetPooled();
            evt.edges = edges;
            evt.Initialize();
            return evt;

        }
    }

    public class ObjectGraphValidateEvent : EventBase<ObjectGraphValidateEvent> {

        public bool isValid { get; private set; }
        public ObjectGraphValidateEvent() {
            base.Init();
        }
        public static ObjectGraphValidateEvent GetPooled(bool status) {
            var evt = GetPooled();
            evt.isValid = status;
            return evt;
        }
    }
}