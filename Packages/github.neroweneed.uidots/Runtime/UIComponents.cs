using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace NeroWeNeed.UIDots {
    public unsafe struct UIConfiguration : IComponentData {
        public int index;
        public void* configData;
        public int ConfigLength { get => UnsafeUtility.ReadArrayElement<int>(configData, 0); }
    }
    public unsafe struct UIRoot : IComponentData {
        public BlobAssetReference<UIGraph> graph;
        public void* configuration;
        public long length;
        public long allocatedLength;
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