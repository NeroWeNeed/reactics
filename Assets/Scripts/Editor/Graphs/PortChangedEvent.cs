using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph
{
    public class PortChangedEvent : EventBase<PortChangedEvent>
    {

        public IEnumerable<Edge> edges { get; private set; }
        public PortChangedEvent()
        {
            base.Init();
            Initialize();
        }
        private void Initialize()
        {
            var prop = typeof(PortChangedEvent).GetProperty("propagation", BindingFlags.Instance |
                            BindingFlags.NonPublic |
                            BindingFlags.Public);
            prop?.SetValue(this, 3);
        }
        public static PortChangedEvent GetPooled(IEnumerable<Edge> edges)
        {
            var evt = GetPooled();
            evt.edges = edges;
            evt.Initialize();
            return evt;

        }
    }
}