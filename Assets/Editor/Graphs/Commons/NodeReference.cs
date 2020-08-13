using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Reactics.Core.Editor.Graph {
    public struct NodeReference : IObjectGraphNodeValueCopyCallback {

        public string nodeId;

        public NodeReference(string nodeId) {
            this.nodeId = nodeId;
        }
        public NodeReference(NodeReference nodeReference) {
            this.nodeId = nodeReference.nodeId;
        }

        public object OnCopy(ObjectGraphNodeJsonSet.Entry[] entries, string[] newIds, string master) {

            if (nodeId == master)
                return new NodeReference(this);
            var myId = nodeId;
            var newId = Array.FindIndex(entries, (entry) => entry.id == myId);
            Debug.Log(newId);
            if (newId >= 0) {
                return new NodeReference(newIds[newId]);
            }
            else
                return new NodeReference(null);

        }
        public override string ToString() {
            return $"NodeReference({nodeId})";
        }
    }
}