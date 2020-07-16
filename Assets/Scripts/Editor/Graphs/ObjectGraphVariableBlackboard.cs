using UnityEditor.Experimental.GraphView;

namespace Reactics.Editor.Graph {
    public class ObjectGraphVariableBlackboard : Blackboard {
        public ObjectGraphVariableBlackboard(GraphView associatedGraphView = null) : base(associatedGraphView) {
            capabilities = Capabilities.Ascendable | Capabilities.Collapsible | Capabilities.Movable | Capabilities.Resizable | Capabilities.Selectable;
        }
    }
}