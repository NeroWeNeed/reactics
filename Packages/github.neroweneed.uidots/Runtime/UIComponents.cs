using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace NeroWeNeed.UIDots {
    public struct UIConfiguration : IComponentData {
        public int index;
        public int submesh;
    }
    public struct UIByteData : IBufferElementData {
        public byte value;

        public override bool Equals(object obj) {
            return obj is UIByteData data &&
                   value == data.value;
        }

        public override int GetHashCode() {
            return -1584136870 + value.GetHashCode();
        }
    }

    public unsafe struct UIRoot : IComponentData {
        public BlobAssetReference<UIGraph> graph;

        public UIRoot(BlobAssetReference<UIGraph> graph) {
            this.graph = graph;
        }
    }
    public struct UIDirtyState : ISharedComponentData, IEquatable<UIDirtyState> {

        public bool value;
        public UIDirtyState(bool value) {
            this.value = value;
        }

        public bool Equals(UIDirtyState other) {
            return this.value == other.value;
        }
        public override bool Equals(object obj) {
            return ((UIDirtyState)obj).value == value;
        }
        public override int GetHashCode() {
            return -1584136870 + value.GetHashCode();
        }
        public static implicit operator UIDirtyState(bool value) => new UIDirtyState(value);
    }
    public struct UINode : IBufferElementData {
        public Entity value;

        public UINode(Entity value) {
            this.value = value;
        }
    }
    public struct UIParent : IComponentData {
        public Entity value;

        public UIParent(Entity value) {
            this.value = value;
        }
    }


}