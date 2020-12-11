using System;
using Unity.Burst;
using Unity.Entities;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public struct BehaviourNode<TBehaviour> {
        public FunctionPointer<TBehaviour> action;
        public int next;
        public long dataLength;
    }
    public unsafe struct BehaviourVariableDefinition {
        public int sourceFieldOffset;
        public long sourceFieldLength;
        public int destination;
        public long destinationLength;
    }
    public struct BehaviourGraph<TBehaviour> {
        public BlobArray<BehaviourNode<TBehaviour>> nodes;
        public BlobArray<BehaviourVariableDefinition> variables;
        public BlobArray<int> roots;
        public BlobArray<byte> data;
    }

}