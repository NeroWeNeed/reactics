using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace NeroWeNeed.UIDots {
    [BurstCompile]
    public unsafe struct UICollectInheritableFieldJob : IJobChunk {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public BlobAssetReference<CompiledUISchema> schema;

        [ReadOnly]
        public EntityTypeHandle entityHandle;
        [ReadOnly]
        public ComponentTypeHandle<UIGraphData> graphHandle;

        public BufferTypeHandle<UIInheritableFieldData> inheritableFieldDataHandle;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var graphs = chunk.GetNativeArray(graphHandle);
            var fields = chunk.GetBufferAccessor(inheritableFieldDataHandle);

            //var graph = graphs[index];
            for (int index = 0; index < graphs.Length; index++) {
                fields[index].Clear();
                for (int nodeIndex = 0; nodeIndex < graphs[index].GetNodeCount(); nodeIndex++) {
                    for (int inheritableFieldIndex = 0; inheritableFieldIndex < schema.Value.inheritableFields.Length; inheritableFieldIndex++) {
                        var inheritableFieldData = schema.Value.inheritableFields[inheritableFieldIndex];
                        if (graphs[index].HasConfigBlock(nodeIndex, inheritableFieldData.config)) {
                            fields[index].Add(new UIInheritableFieldData { field = inheritableFieldData, index = nodeIndex });
                        }
                    }
                }
            }
        }
    }
}