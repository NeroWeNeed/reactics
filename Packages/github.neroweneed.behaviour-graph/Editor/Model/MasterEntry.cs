using System;
using NeroWeNeed.Commons.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor.Model {
    [Serializable]
    public class MasterEntry : TerminalEntry<BehaviourGraphNode> {

        public const string NODE_CLASS = "master-node";
        public MasterEntry(string id, Rect layout) {
            Id = id;
            Layout = layout;
        }
        public MasterEntry(string id) : this(id, new Rect(BehaviourGraphModel.DEFAULT_SIZE / -2, BehaviourGraphModel.DEFAULT_SIZE)) { }

        public MasterEntry(Rect layout) : this(Guid.NewGuid().ToString("N"), layout) { }
        public MasterEntry() : this(Guid.NewGuid().ToString("N"), new Rect(BehaviourGraphModel.DEFAULT_SIZE / -2, BehaviourGraphModel.DEFAULT_SIZE)) { }
        public override Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            var node = base.CreateNode(graphView, settings);
            node.title = "Master";
            node.AddToClassList(NODE_CLASS);
            node.viewDataKey = Id;
            node.SetPosition(Layout);
            node.capabilities = Capabilities.Ascendable | Capabilities.Collapsible | Capabilities.Movable | Capabilities.Snappable | Capabilities.Selectable;
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(BehaviourPort<>).MakeGenericType(settings.BehaviourType));
            port.portName = string.IsNullOrEmpty(settings.behaviourName) ? "Behaviour" : settings.behaviourName;
            port.portColor = settings.BehaviourType.GetColor();
            port.AddToClassList(IntermediateEntry.INPUT_PORT);
            node.inputContainer.Add(port);
            return node;
        }


    }
}
