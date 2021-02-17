using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
    public struct UIPixelScale : IComponentData {
        public float value;
    }



    public struct UINodeInfo : IComponentData {
        public int index;
        public int submesh;
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
    public struct UICameraContextDirty : ISharedComponentData {
        public bool value;

        public UICameraContextDirty(bool value) {
            this.value = value;
        }

        public static implicit operator UICameraContextDirty(bool value) => new UICameraContextDirty(value);
    }
    [WriteGroup(typeof(LocalToWorld))]
    public struct LocalToCamera : IComponentData, IEquatable<LocalToCamera> {
        public float4x4 cameraLTW;
        public float2 clipPlane;
        public UILength offsetX, offsetY;
        public Alignment alignment;

        public override bool Equals(object obj) {
            return obj is LocalToCamera camera &&
                   cameraLTW.Equals(camera.cameraLTW) &&
                   clipPlane.Equals(camera.clipPlane) &&
                   EqualityComparer<UILength>.Default.Equals(offsetX, camera.offsetX) &&
                   EqualityComparer<UILength>.Default.Equals(offsetY, camera.offsetY) &&
                   alignment == camera.alignment;
        }
        public bool Equals(LocalToCamera other) {
            return cameraLTW.Equals(other.cameraLTW) &&
                    clipPlane.Equals(other.clipPlane) &&
                    EqualityComparer<UILength>.Default.Equals(offsetX, other.offsetX) &&
                    EqualityComparer<UILength>.Default.Equals(offsetY, other.offsetY) &&
                    alignment == other.alignment;
        }

        public override int GetHashCode() {
            int hashCode = -1241407421;
            hashCode = hashCode * -1521134295 + cameraLTW.GetHashCode();
            hashCode = hashCode * -1521134295 + clipPlane.GetHashCode();
            hashCode = hashCode * -1521134295 + offsetX.GetHashCode();
            hashCode = hashCode * -1521134295 + offsetY.GetHashCode();
            hashCode = hashCode * -1521134295 + alignment.GetHashCode();
            return hashCode;
        }
    }
    public struct UISchemaData : ISharedComponentData, IEquatable<UISchemaData> {
        public UISchema value;

        public override bool Equals(object obj) {
            return obj is UISchemaData data &&
                   EqualityComparer<UISchema>.Default.Equals(value, data.value);
        }

        public bool Equals(UISchemaData other) {
            return EqualityComparer<UISchema>.Default.Equals(value, other.value);
        }

        public override int GetHashCode() {
            return -1584136870 + EqualityComparer<UISchema>.Default.GetHashCode(value);
        }
    }
    public struct UICompiledSchemaData : ISystemStateComponentData {
        public BlobAssetReference<CompiledUISchema> value;
    }
    public struct UIInheritableFieldData : IBufferElementData {
        public UISchema.InheritableField field;
        public int index;
    }
    public struct UIContext : IComponentData { }
    public struct UIContextData : IComponentData, ISystemStateComponentData {
        public const float WORLD_PIXEL_SCALE = 0.001f;
        public float dpi;
        public float pixelScale;
        public float2 size;
        public float relativeTo;

        public static UIContextData CreateContext(Camera camera = null) {

            return new UIContextData
            {
                dpi = Screen.dpi,
                pixelScale = camera == null ? WORLD_PIXEL_SCALE : 1f,
                size = camera == null ? new float2(float.PositiveInfinity, float.PositiveInfinity) : new float2(camera.orthographicSize * camera.aspect * 2, camera.orthographicSize * 2)
            };
        }

    }
    public struct FollowCameraData : IComponentData {
        public Entity value;
    }
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