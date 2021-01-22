using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace NeroWeNeed.UIDots {
    public struct UIGraph : IComponentData {
        public BlittableAssetReference value;
    }
    public struct UIGraphHandleData : ISystemStateComponentData {
        public int id;
    }
    public struct UIGraphData : ISystemStateComponentData {
        public IntPtr value;
        public long allocatedLength;
        public bool IsCreated { get => allocatedLength > 0; }
    }
    public struct UIHandle : IComponentData {
        public int handle;

        public UIHandle(int handle) {
            this.handle = handle;
        }
    }

    public struct UINodeInfo : IComponentData {
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
        public BlobAssetReference<UIGraphOld> graph;
        public Entity contextSource;

        public UIRoot(BlobAssetReference<UIGraphOld> graph, Entity contextSource) {
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
    public struct UICursor : IComponentData {
        public Entity target;
        public int index;
    }
    public struct UICursorInput : IComponentData {
        public float2 direction;
        /// <summary>
        /// Multiplied against all the axis involved (set the x or y to 0 to for vertical or horizontal navigation)
        /// </summary>
        public float2 multiplier;
        public float accuracy;
        //Infinity/Nan will result in the cursor moving immediately.
        public float speed;
        public bool selected;

        public UICursorInput(float2 direction, float2 multiplier, float accuracy = math.PI / 6f, float speed = 1f) {
            this.direction = direction;
            this.multiplier = multiplier;
            this.accuracy = accuracy;
            this.speed = speed;
            this.selected = false;
        }
    }
    public struct UIOnSelect : IComponentData {
        public FunctionPointer<UISelectDelegate> value;
    }

    public struct UISelectable : IComponentData {
    }
    public struct UICursorDirty : IComponentData {
        public bool value;
        public UICursorDirty(bool dirty) {
            this.value = dirty;
        }

    }

}