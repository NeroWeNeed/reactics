using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Mesh;

namespace NeroWeNeed.UIDots {
    [BurstCompile]
    public unsafe struct UpdateUIConfiguration : IJobChunk {
        public ComponentTypeHandle<UIRoot> rootHandle;
        public BufferTypeHandle<UINode> nodeHandle;
        public ComponentDataFromEntity<UIConfiguration> configHandle;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            var roots = chunk.GetNativeArray(rootHandle);
            var buffers = chunk.GetBufferAccessor(nodeHandle);
            for (int i = 0; i < buffers.Length; i++) {
                var nodes = new NativeArray<ConfigInfo>(roots[i].graph.Value.nodes.Length, Allocator.Temp);
                var size = 0;
                var offset = 0;
                var nodeIndex = 0;
                while (offset < roots[i].length) {
                    var cSize = UnsafeUtility.AsRef<int>((((IntPtr)roots[i].configuration) + offset).ToPointer());
                    nodes[nodeIndex] = new ConfigInfo(cSize, (((IntPtr)roots[i].configuration) + offset + UnsafeUtility.SizeOf<int>()).ToPointer());
                    offset += cSize + UnsafeUtility.SizeOf<int>();
                    nodeIndex++;
                }
                for (int j = 0; j < buffers[i].Length; j++) {
                    var entityConfig = configHandle[buffers[i][j].value];
                    nodes[entityConfig.index] = new ConfigInfo(entityConfig.ConfigLength, (((IntPtr)entityConfig.configData) + UnsafeUtility.SizeOf<int>()).ToPointer());
                }
                foreach (var node in nodes) {
                    size += node.length + UnsafeUtility.SizeOf<int>();
                }
                if (size <= roots[i].allocatedLength) {
                    UnsafeUtility.MemClear(roots[i].configuration, roots[i].allocatedLength);
                    roots[i] = new UIRoot
                    {
                        configuration = roots[i].configuration,
                        allocatedLength = roots[i].allocatedLength,
                        length = size,
                        graph = roots[i].graph
                    };
                }
                else {
                    UnsafeUtility.Free(roots[i].configuration, Allocator.Persistent);
                    roots[i] = new UIRoot
                    {
                        configuration = UnsafeUtility.Malloc(size, 0, Allocator.Persistent),
                        allocatedLength = size,
                        length = size,
                        graph = roots[i].graph
                    };
                }
                offset = 0;
                for (int j = 0; j < roots[i].graph.Value.nodes.Length; j++) {
                    var l = nodes[j].length;
                    UnsafeUtility.CopyStructureToPtr(ref l, (((IntPtr)roots[i].configuration) + offset).ToPointer());
                    UnsafeUtility.MemCpy(nodes[j].config, (((IntPtr)roots[i].configuration) + offset + UnsafeUtility.SizeOf<int>()).ToPointer(), l);
                }

            }

        }
        private struct ConfigInfo {
            public int length;
            public void* config;

            public ConfigInfo(int length, void* config) {
                this.length = length;
                this.config = config;
            }
        }
    }
    [BurstCompile]
    public unsafe struct EmitUIEntitiesJob : IJob {
        public BlobAssetReference<UIGraph> graph;
        [NativeDisableUnsafePtrRestriction]
        public void* configuration;
        public EntityCommandBuffer entityCommandBuffer;
        [WriteOnly]
        public NativeArray<Entity> entities;
        public void Execute() {
            var root = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent<UIRoot>(root);
            var children = entityCommandBuffer.AddBuffer<UINode>(root);
            int offset = 0;
            var ptr = (IntPtr)configuration;
            var entities = new NativeList<Entity>(4, Allocator.TempJob);
            for (int i = 0; i < graph.Value.nodes.Length; i++) {
                var size = UnsafeUtility.AsRef<int>((ptr + offset).ToPointer());
                UIConfig* dataPtr = (UIConfig*)(ptr + offset + UnsafeUtility.SizeOf<int>()).ToPointer();
                if (dataPtr->name.IsCreated) {
                    var node = entityCommandBuffer.CreateEntity();
                    var config = new UIConfiguration
                    {
                        index = i,
                        configData = UnsafeUtility.Malloc(size + UnsafeUtility.SizeOf<int>(), 0, Allocator.Persistent)
                    };
                    var entity = entityCommandBuffer.CreateEntity();
                    entityCommandBuffer.AddComponent(entity, config);
                    entityCommandBuffer.AddComponent(entity, new UIParent(root));
                    children.Add(new UINode(entity));
                    UnsafeUtility.CopyStructureToPtr<int>(ref size, config.configData);
                    UnsafeUtility.MemCpy(((IntPtr)config.configData + UnsafeUtility.SizeOf<int>()).ToPointer(), (ptr + offset + UnsafeUtility.SizeOf<int>()).ToPointer(), size);
                    entityCommandBuffer.AddComponent<UIConfiguration>(node, config);
                }
                offset += UnsafeUtility.SizeOf<int>() + size;
            }
            this.entities = entities.AsArray();
        }

    }

    [BurstCompile]
    public unsafe struct UILayoutJob : IJob {
        [ReadOnly]
        public BlobAssetReference<UIGraph> graph;
        [NativeDisableUnsafePtrRestriction]
        [ReadOnly]
        public void* configuration;
        public int length;

        public MeshDataArray meshData;
        public void Execute() {
            if (graph.Value.nodes.Length > 0) {
                var statePtr = UnsafeUtility.Malloc((graph.Value.nodes.Length + 1) * UnsafeUtility.SizeOf<UIPassState>(), 0, Allocator.TempJob);
                for (int i = 0; i < graph.Value.nodes.Length + 1; i++) {
                    UnsafeUtility.WriteArrayElement(statePtr, i, UIPassState.DEFAULT);
                }
                var defaultState = UIPassState.DEFAULT;
                UnsafeUtility.CopyStructureToPtr(ref defaultState, statePtr);
                var configPtr = configuration;
                var configLayout = GetConfigLayout((IntPtr)configuration);
                var renderBoxCount = GetRenderBoxLayout(configPtr, configLayout, out NativeArray<OffsetInfo> renderBoxLayout);
                InitMeshData(meshData[0], renderBoxCount);
                var vertexPtr = UnsafeUtility.Malloc(renderBoxCount * 4 * UnsafeUtility.SizeOf<UIVertexData>(), 0, Allocator.TempJob);
                var context = new UILengthContext
                {
                    dpi = 96
                };
                var contextPtr = (UILengthContext*)UnsafeUtility.AddressOf(ref context);
                var indices = meshData[0].GetIndexData<ushort>();
                Layout(0, statePtr, configPtr, vertexPtr, renderBoxLayout, contextPtr, configLayout, float4.zero);
                Render(statePtr, configPtr, vertexPtr, renderBoxLayout, contextPtr, configLayout, indices);
                var vertices = meshData[0].GetVertexData<UIVertexData>();
                meshData[0].SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, renderBoxCount * 6, MeshTopology.Triangles));
                UnsafeUtility.MemCpy(vertices.GetUnsafePtr(), vertexPtr, renderBoxCount * 4 * UnsafeUtility.SizeOf<UIVertexData>());
                UnsafeUtility.Free(statePtr, Allocator.TempJob);
                UnsafeUtility.Free(vertexPtr, Allocator.TempJob);
                renderBoxLayout.Dispose();
                configLayout.Dispose();
            }
        }
        private NativeArray<OffsetInfo> GetConfigLayout(IntPtr configuration, Allocator allocator = Allocator.TempJob) {
            var config = new NativeArray<OffsetInfo>(graph.Value.nodes.Length, allocator);
            var offset = 0;
            for (int i = 0; i < graph.Value.nodes.Length; i++) {
                var size = UnsafeUtility.AsRef<int>((configuration + offset).ToPointer());
                offset += UnsafeUtility.SizeOf<int>();
                config[i] = new OffsetInfo(offset, size);
                offset += size;
            }
            return config;
        }
        private void InitMeshData(MeshData meshData, int renderBoxCount) {
            var parameters = UIVertexData.AllocateVertexDescriptor(Allocator.Temp);
            meshData.SetVertexBufferParams(renderBoxCount * 4, parameters);
            meshData.SetIndexBufferParams(renderBoxCount * 6, UnityEngine.Rendering.IndexFormat.UInt16);
            meshData.subMeshCount = 1;


        }
        private int GetRenderBoxLayout(void* configPtr, NativeArray<OffsetInfo> configLayout, out NativeArray<OffsetInfo> offsets, Allocator allocator = Allocator.TempJob) {
            offsets = new NativeArray<OffsetInfo>(graph.Value.nodes.Length, allocator);
            int count = 0;
            for (int i = 0; i < graph.Value.nodes.Length; i++) {
                offsets[i] = new OffsetInfo(count, graph.Value.nodes[i].renderBoxHandler.IsCreated ? graph.Value.nodes[i].renderBoxHandler.Invoke(configPtr, configLayout[i].offset, configLayout[i].length) : 1);
                count += offsets[i].length;
            }
            return count;
        }
        private void Render(void* statePtr, void* configPtr, void* vertexPtr, NativeArray<OffsetInfo> renderBoxLayout, UILengthContext* context, NativeArray<OffsetInfo> configLayout, NativeArray<ushort> indices) {
            for (int i = 0; i < graph.Value.nodes.Length; i++) {
                graph.Value.nodes[i].pass.Invoke(
                    (byte)UIPassType.Render,
                    configPtr,
                    configLayout[i].offset,
                    configLayout[i].length,
                    statePtr,
                    (int*)graph.Value.nodes[i].children.GetUnsafePtr(),
                    i + 1,
                    -1,
                    graph.Value.nodes[i].children.Length,
                    vertexPtr,
                    renderBoxLayout[i].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                    context
                );
                for (int j = 0; j < renderBoxLayout[i].length; j++) {
                    indices[(renderBoxLayout[i].offset + j) * 6] = (ushort)((renderBoxLayout[i].offset + j) * 4);
                    indices[((renderBoxLayout[i].offset + j) * 6) + 1] = (ushort)(((renderBoxLayout[i].offset + j) * 4) + 2);
                    indices[((renderBoxLayout[i].offset + j) * 6) + 2] = (ushort)(((renderBoxLayout[i].offset + j) * 4) + 1);
                    indices[((renderBoxLayout[i].offset + j) * 6) + 3] = (ushort)(((renderBoxLayout[i].offset + j) * 4) + 2);
                    indices[((renderBoxLayout[i].offset + j) * 6) + 4] = (ushort)(((renderBoxLayout[i].offset + j) * 4) + 3);
                    indices[((renderBoxLayout[i].offset + j) * 6) + 5] = (ushort)(((renderBoxLayout[i].offset + j) * 4) + 1);
                }
            }
        }
        private void Layout(int currentIndex, void* statePtr, void* configPtr, void* vertexPtr, NativeArray<OffsetInfo> offsets, UILengthContext* context, NativeArray<OffsetInfo> configLayout, float4 offset) {
            var config = (UIConfig*)((((IntPtr)configPtr) + configLayout[currentIndex].offset).ToPointer());
            var state = (UIPassState*)(((IntPtr)statePtr) + UnsafeUtility.SizeOf<UIPassState>() * (currentIndex + 1)).ToPointer();
            state->margin = config->margin.Normalize(*context);
            state->localOffset = new float2(state->margin.x, state->margin.y);
            state->padding = config->padding.Normalize(*context);
            offset += state->margin;
            state->globalBox = offset;

            graph.Value.nodes[currentIndex].pass.Invoke(
                (byte)UIPassType.LayoutSelf,
                configPtr,
                configLayout[currentIndex].offset,
                configLayout[currentIndex].length,
                statePtr,
                (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                currentIndex + 1,
                -1,
                graph.Value.nodes[currentIndex].children.Length,
                vertexPtr,
                offsets[currentIndex].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                context
            );

            for (int index = 0; index < graph.Value.nodes[currentIndex].children.Length; index++) {
                var innerConfig = (UIConfig*)((((IntPtr)configPtr) + configLayout[graph.Value.nodes[currentIndex].children[index]].offset).ToPointer());
                var innerState = (UIPassState*)(((IntPtr)statePtr) + UnsafeUtility.SizeOf<UIPassState>() * (graph.Value.nodes[currentIndex].children[index] + 1)).ToPointer();
                innerState->globalBox = offset + state->padding;
                graph.Value.nodes[currentIndex].pass.Invoke(
                    (byte)UIPassType.LayoutChild,
                    configPtr,
                    configLayout[currentIndex].offset,
                    configLayout[currentIndex].length,
                    statePtr,
                    (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                    currentIndex + 1,
                    index,
                    graph.Value.nodes[currentIndex].children.Length,
                    vertexPtr,
                    offsets[currentIndex].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                    context
                );
                Layout(graph.Value.nodes[currentIndex].children[index], statePtr, configPtr, vertexPtr, offsets, context, configLayout, offset + state->padding);
                graph.Value.nodes[currentIndex].pass.Invoke(
                    (byte)UIPassType.SizeChild,
                    configPtr,
                    configLayout[currentIndex].offset,
                    configLayout[currentIndex].length,
                    statePtr,
                    (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                    currentIndex + 1,
                    index,
                    graph.Value.nodes[currentIndex].children.Length,
                    vertexPtr,
                    offsets[currentIndex].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                    context
                );
            }
            graph.Value.nodes[currentIndex].pass.Invoke(
                (byte)UIPassType.SizeSelf,
                configPtr,
                configLayout[currentIndex].offset,
                configLayout[currentIndex].length,
                statePtr,
                (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                currentIndex + 1,
                -1,
                graph.Value.nodes[currentIndex].children.Length,
                vertexPtr,
                offsets[currentIndex].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                context
            );
            state->size = new float2(
                    math.clamp(state->size.x, config->size.minWidth.Normalize(*context), config->size.maxWidth.Normalize(*context)) + state->padding.x + state->padding.z,
                    math.clamp(state->size.y, config->size.minHeight.Normalize(*context), config->size.maxHeight.Normalize(*context)) + state->padding.y + state->padding.w);

        }
        private struct OffsetInfo {
            public int offset;
            public int length;

            public OffsetInfo(int offset, int length) {
                this.offset = offset;
                this.length = length;
            }
        }

    }
}