using System;
using UnityEngine;

namespace Reactics.Editor.Graph {
    [Serializable]
    public struct BehaviourModelLayout {
        public Rect master;
        public NodeLayout[] nodes;
        public VariableLayout[] variables;
    }

    [Serializable]
    public struct NodeLayout {
        public Rect layout;
        public int index;
        public NodeLayout(Rect layout, int index) {
            this.layout = layout;
            this.index = index;
        }
    }
    [Serializable]
    public struct VariableLayout {
        public Rect layout;
        public int index;
        public string typeName;
        public NodeConnection[] connections;

        public VariableLayout(Rect layout, int index, string typeName, NodeConnection[] connections) {
            this.layout = layout;
            this.index = index;
            this.typeName = typeName;
            this.connections = connections;
        }
    }
    [Serializable]
    public struct NodeConnection {
        public int index;
        public string field;

        public NodeConnection(int index, string field) {
            this.index = index;
            this.field = field;
        }
    }
}