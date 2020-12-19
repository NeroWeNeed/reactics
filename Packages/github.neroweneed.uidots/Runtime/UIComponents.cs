using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

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
        public Entity contextSource;

        public UIRoot(BlobAssetReference<UIGraph> graph,Entity contextSource) {
            this.graph = graph;
            this.contextSource = contextSource;
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
        public static implicit operator UINode(Entity e) => new UINode(e);
    }
    public struct UIParent : IComponentData {
        public Entity value;

        public UIParent(Entity value) {
            this.value = value;
        }
        public static implicit operator UIParent(Entity e) => new UIParent(e);
    }
    public struct UIScreenElement : IComponentData {
        public UILength x, y;
        public Alignment alignment;
    }
    public struct UICameraContext : IComponentData {
        public float4x4 cameraLTW;
        public float2 clipPlane;

    }
    public struct UIContextProvider : IComponentData { }
    public struct UIContextSource : IComponentData {
        public Entity value;

        public UIContextSource(Entity value) {
            this.value = value;
        }
    }
    public struct UIFaceScreen : IComponentData { }
    public struct UICameraLayerData : IComponentData {

    }

}