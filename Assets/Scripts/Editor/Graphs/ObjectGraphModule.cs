using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Reactics.Editor.Graph
{

    public interface IMasterNodeModule
    {
        void ConfigureMaster(Node master);
    }
    public abstract class ObjectGraphModule<TNode> : IMasterNodeModule where TNode : ObjectGraphNode
    {

        protected ObjectGraphModel model = UnityEngine.ScriptableObject.CreateInstance<ObjectGraphModel>();
        public ObjectGraphModel Model { get => model; }


        public abstract TNode CreateNode(Type type, Rect layout);
        public abstract ObjectGraphNodeSet<TNode> CollectNodes(Node master);

        public abstract void ConfigureMaster(Node master);
    }
    public struct ObjectGraphNodeSet<TNode> where TNode : ObjectGraphNode
    {
        public TNode[] nodes;
        public int[] roots;
    }
}