using UnityEditor.Experimental.GraphView;

namespace NeroWeNeed.BehaviourGraph.Editor.Model {
    public abstract class TerminalEntry : BaseEntry {
        public override int Priority { get => 1; }

    }
    public abstract class TerminalEntry<TNode> : TerminalEntry where TNode : Node, new() {
        public override Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            TNode node = new TNode
            {
                viewDataKey = Id
            };
            node.SetPosition(Layout);
            return node;
        }
    }
}