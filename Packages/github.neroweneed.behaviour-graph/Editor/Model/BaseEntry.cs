using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace NeroWeNeed.BehaviourGraph.Editor.Model {
    public abstract class BaseEntry : IEntry {
        [SerializeField]
        private string id;
        [SerializeField]
        private Rect layout;
        public virtual int Priority { get => 0; }
        public string Id { get => id; set => id = value; }
        public Rect Layout { get => layout; set => layout = value; }
        public virtual int CompareTo(IEntry other) {
            if (other == null)
                return -1;
            return other.Priority.CompareTo(this.Priority);
        }

        public virtual Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            Node node = new Node();
            node.AddToClassList(this.GetType().Name);
            node.viewDataKey = Id;
            node.SetPosition(Layout);
            return node;
        }
    }
    public abstract class BaseEntry<TNode> : BaseEntry where TNode : Node, new() {

        public override Node CreateNode(BehaviourGraphView graphView, BehaviourGraphSettings settings) {
            TNode node = new TNode
            {
                viewDataKey = Id
            };
            node.AddToClassList(this.GetType().Name);
            node.SetPosition(Layout);
            return node;
        }
    }
}