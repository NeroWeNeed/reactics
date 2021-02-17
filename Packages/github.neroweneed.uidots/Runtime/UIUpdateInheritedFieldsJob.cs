using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace NeroWeNeed.UIDots {
    public unsafe struct UIUpdateInheritedFieldsJob : IJobChunk {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public BlobAssetReference<CompiledUISchema> schema;

        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public ComponentTypeHandle<UIGraphData> graphHandle;
        [ReadOnly]
        public BufferTypeHandle<UIInheritableFieldData> inheritableFieldDataHandle;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var graphs = chunk.GetNativeArray(graphHandle);
            var fields = chunk.GetBufferAccessor(inheritableFieldDataHandle);

            //var graph = graphs[index];
            for (int index = 0; index < graphs.Length; index++) {
                foreach (var field in fields[index]) {
                    var targetNodeIndex = field.index;
                    var header = graphs[index].GetNodeHeader(targetNodeIndex);
                    var targetField = graphs[index].GetConfigBlock(targetNodeIndex, field.field.config);
                    var parent = header.parent;
                    while (parent >= 0) {
                        if (graphs[index].TryGetConfigBlock(parent, field.field.config, out IntPtr result)) {
                            var inheritableField = UnsafeUtility.AsRef<UILength>((result + field.field.offset).ToPointer());
                            if (inheritableField.unit != UILengthUnit.Auto && inheritableField.unit != UILengthUnit.Inherit) {
                                UnsafeUtility.MemCpy((targetField + field.field.offset).ToPointer(), UnsafeUtility.AddressOf(ref inheritableField), field.field.length);
                                break;
                            }
                        }
                        parent = graphs[index].GetNodeHeader(parent).parent;
                    }
                }
            }
        }
    }
}