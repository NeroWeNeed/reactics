using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Mesh;

namespace NeroWeNeed.UIDots {
    /*     [BurstCompile]
        public unsafe struct UpdateUIConfigurationJob : IJobChunk {
            [ReadOnly]
            public EntityTypeHandle entityHandle;
            [ReadOnly]
            public ComponentTypeHandle<UIRoot> rootHandle;
            [NativeDisableParallelForRestriction]
            public BufferFromEntity<UIByteData> byteAccessor;
            [ReadOnly]
            public BufferTypeHandle<UINode> nodeHandle;
            [ReadOnly]
            public ComponentDataFromEntity<UIConfiguration> configHandle;
            [NativeDisableParallelForRestriction]
            public ComponentTypeHandle<UIDirtyState> dirtyHandle;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var entities = chunk.GetNativeArray(entityHandle);
                var roots = chunk.GetNativeArray(rootHandle);
                var buffers = chunk.GetBufferAccessor(nodeHandle);
                var dirty = chunk.GetNativeArray(dirtyHandle);
                for (int i = 0; i < entities.Length; i++) {
                    if (dirty[i].value == DirtyState.Clean)
                        continue;
                    var rootBuffer = byteAccessor[entities[i]].Reinterpret<byte>();
                    var srcEntities = new NativeArray<Entity>(buffers[i].Length + 1, Allocator.Temp);
                    srcEntities[0] = entities[i];
                    var nodes = new NativeArray<ConfigInfo>(roots[i].graph.Value.nodes.Length, Allocator.Temp);
                    var size = 0;
                    var offset = 0;
                    var nodeIndex = 0;
                    var bufferPtr = rootBuffer.GetUnsafePtr();
                    while (offset < rootBuffer.Length) {
                        UnsafeUtility.CopyPtrToStructure((((IntPtr)bufferPtr) + offset).ToPointer(), out int currentSize);
                        nodes[nodeIndex] = new ConfigInfo(currentSize, offset + UnsafeUtility.SizeOf<int>(), 0);
                        offset += currentSize + UnsafeUtility.SizeOf<int>();
                        nodeIndex++;
                    }
                    for (int j = 0; j < buffers[i].Length; j++) {
                        var entityConfig = configHandle[buffers[i][j].value];
                        var entityBuffer = byteAccessor[buffers[i][j].value].Reinterpret<byte>();
                        nodes[entityConfig.index] = new ConfigInfo(entityBuffer.Length, 0, j + 1);
                    }
                    foreach (var node in nodes) {
                        size += node.length + UnsafeUtility.SizeOf<int>();
                    }
                    NativeArray<byte> output = new NativeArray<byte>(size, Allocator.Temp);
                    //offset = 0;
                    Debug.Log(size);
                    for (int j = 0; j < nodes.Length; j++) {
                        int l = nodes[j].length;

                        UnsafeUtility.CopyStructureToPtr(ref l, (((IntPtr)output.GetUnsafePtr()) + nodes[j].offset).ToPointer());
                        UnsafeUtility.MemCpy(output.GetUnsafePtr(), (((IntPtr)byteAccessor[srcEntities[nodes[j].entityIndex]].GetUnsafePtr()) + nodes[j].offset + UnsafeUtility.SizeOf<int>()).ToPointer(), nodes[j].length);
                        //offset += UnsafeUtility.SizeOf<int>() + l;
                    }
                    var different = IsDifferent(output, rootBuffer.AsNativeArray());
                    if (different) {
                        rootBuffer.Clear();
                        rootBuffer.EnsureCapacity(output.Length);
                        rootBuffer.AddRange(output);
                        dirty[i] = new UIDirtyState(DirtyState.BytesChanged);
                    }
                    else {
                        dirty[i] = new UIDirtyState(DirtyState.Clean);
                    }

                }

            }
            private bool IsDifferent(NativeArray<byte> b1, NativeArray<byte> b2) {
                return b1.Length != b2.Length || UnsafeUtility.MemCmp(b1.GetUnsafePtr(), b2.GetUnsafePtr(), b1.Length) != 0;
            }
            private int Diff(NativeArray<byte> b1, NativeArray<byte> b2) {
                var stride = math.min(b1.Length, b2.Length);
                var sizeDif = math.abs(b1.Length - b2.Length);
                for (int i = 0; i < stride; i++) {
                    sizeDif += b1[i] - b2[i];
                }
                return sizeDif;
            }
            private struct ConfigInfo {
                public int length;
                public int offset;

                public int entityIndex;

                public ConfigInfo(int length, int offset, int entityIndex) {
                    this.length = length;
                    this.offset = offset;
                    this.entityIndex = entityIndex;
                }
            }
        } */
    /*     [BurstCompile]
        public unsafe struct EmitUIEntitiesJob : IJob {
            public BlobAssetReference<UIGraph> graph;
            [NativeDisableUnsafePtrRestriction]
            public void* configuration;
            public EntityCommandBuffer entityCommandBuffer;
            [WriteOnly]
            public NativeArray<Entity> entities;
            public void Execute() {
                var root = entityCommandBuffer.CreateEntity();

                var children = entityCommandBuffer.AddBuffer<UINode>(root);
                int offset = 0;
                var ptr = (IntPtr)configuration;
                var entities = new NativeList<Entity>(4, Allocator.TempJob);
                int submesh = 1;
                for (int i = 0; i < graph.Value.nodes.Length; i++) {
                    var size = UnsafeUtility.AsRef<int>((ptr + offset).ToPointer());
                    UIConfig* dataPtr = (UIConfig*)(ptr + offset + UnsafeUtility.SizeOf<int>()).ToPointer();
                    if (dataPtr->name.IsCreated) {
                        UIConfiguration config = new UIConfiguration
                        {
                            index = i,
                            submesh = ++submesh,
                            configData = UnsafeUtility.Malloc(size + UnsafeUtility.SizeOf<int>(), 0, Allocator.Persistent)
                        };

                        var node = entityCommandBuffer.CreateEntity();
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
                entityCommandBuffer.AddComponent<UIRoot>(root, new UIRoot
                {
                    graph = graph,
                    configuration = configuration
                });
                this.entities = entities.AsArray();
            }

        } */
    public struct UINodeDecompositionJob : IJobParallelFor {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public NativeArray<BlobAssetReference<UIGraph>> graphs;

        [WriteOnly]
        public NativeStream.Writer nodes;
        public void Execute(int index) {
            if (graphs[index].Value.nodes.Length > 0) {
                DecomposeHead(graphs[index], index);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DecomposeHead(BlobAssetReference<UIGraph> graph, int threadIndex) {
            var submeshCount = UIJobUtility.GetConfigLayout(graph, out NativeArray<OffsetInfo> configLayout, Allocator.Temp);
            int currentSubmesh = 0;
            var nodeInfo = new NativeArray<NodeInfo>(submeshCount, Allocator.Temp);
            Decompose(graph, threadIndex, 0, configLayout, ref currentSubmesh, nodeInfo, submeshCount + 1);
            nodes.BeginForEachIndex(threadIndex);
            for (int i = 0; i < nodeInfo.Length; i++) {
                nodes.Write(nodeInfo[i]);
            }
            nodes.EndForEachIndex();

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Decompose(BlobAssetReference<UIGraph> graph, int threadIndex, int currentIndex, NativeArray<OffsetInfo> configLayout, ref int currentSubmesh, NativeArray<NodeInfo> nodes, int subMeshCount) {
            if (UIConfigLayout.HasName(graph.Value.nodes[currentIndex].configurationMask, ((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + configLayout[currentIndex].offset)) {
                nodes[currentSubmesh] = new NodeInfo(threadIndex, configLayout[currentIndex], currentIndex, subMeshCount - (++currentSubmesh));
            }
            for (int i = 0; i < graph.Value.nodes[currentIndex].children.Length; i++) {
                Decompose(graph, threadIndex, graph.Value.nodes[currentIndex].children[i], configLayout, ref currentSubmesh, nodes, subMeshCount);
            }
        }
        public struct NodeInfo {
            public int graphIndex;
            public OffsetInfo location;
            public int graphNodeIndex;
            public int subMesh;

            public NodeInfo(int graphIndex, OffsetInfo location, int graphNodeIndex, int subMesh) {
                this.graphIndex = graphIndex;
                this.location = location;
                this.graphNodeIndex = graphNodeIndex;
                this.subMesh = subMesh;
            }
        }
    }


    [BurstCompile]
    public unsafe struct UILayoutJob : IJobParallelFor {
        [ReadOnly]
        //[DeallocateOnJobCompletion]
        [NativeDisableUnsafePtrRestriction]
        public NativeArray<BlobAssetReference<UIGraph>> graphs;
        [NativeDisableUnsafePtrRestriction]
        public NativeArray<UIContext> contexts;

        [NativeDisableContainerSafetyRestriction]
        [NativeDisableUnsafePtrRestriction]
        [NativeDisableParallelForRestriction]
        public MeshDataArray meshDataArray;



        public void Execute(int index) {
            Execute(graphs[index], meshDataArray[index], index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(BlobAssetReference<UIGraph> graph, MeshData meshData, int index) {
            if (graph.Value.nodes.Length > 0) {
                var stateLayout = new NativeArray<UIPassState>(graph.Value.nodes.Length, Allocator.Temp);
                var initial = UIPassState.DEFAULT;
                UnsafeUtility.MemCpyReplicate(stateLayout.GetUnsafePtr(), UnsafeUtility.AddressOf(ref initial), UnsafeUtility.SizeOf<UIPassState>(), stateLayout.Length);
                int subMeshCount = UIJobUtility.GetConfigLayout(graph, out NativeArray<OffsetInfo> configLayout, Allocator.Temp);
                var renderBoxLayout = new NativeArray<OffsetInfo>(graph.Value.nodes.Length, Allocator.Temp);
                var subMeshes = new NativeArray<SubMeshInfo>(subMeshCount, Allocator.Temp);
                var renderBoxCount = InitRenderBoxLayout(graph, configLayout, renderBoxLayout);

                InitMeshData(meshData, renderBoxCount);
                NativeArray<UIVertexData> vertices = meshData.GetVertexData<UIVertexData>();
                var contextPtr = (UIContext*)((IntPtr)contexts.GetUnsafePtr() + (UnsafeUtility.SizeOf<UIContext>() * index)).ToPointer();
                Layout(0, graph, configLayout, renderBoxLayout, stateLayout, contextPtr, (UIVertexData*)vertices.GetUnsafePtr());
                Render(vertices, contextPtr, meshData, graph, configLayout, renderBoxLayout, stateLayout, subMeshes);
                stateLayout.Dispose();
                configLayout.Dispose();
                renderBoxLayout.Dispose();
                subMeshes.Dispose();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitMeshData(MeshData meshData, int renderBoxCount) {
            var parameters = UIVertexData.AllocateVertexDescriptor(Allocator.Temp);
            meshData.SetVertexBufferParams(renderBoxCount * 4, parameters);
            meshData.SetIndexBufferParams(renderBoxCount * 6, UnityEngine.Rendering.IndexFormat.UInt16);
            meshData.subMeshCount = 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int InitRenderBoxLayout(BlobAssetReference<UIGraph> graph, NativeArray<OffsetInfo> configLayout, NativeArray<OffsetInfo> renderBoxLayout) {
            int count = 0;
            for (int currentIndex = 0; currentIndex < graph.Value.nodes.Length; currentIndex++) {
                var l = graph.Value.nodes[currentIndex].renderBoxHandler.IsCreated ? graph.Value.nodes[currentIndex].renderBoxHandler.Invoke((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr(), configLayout[currentIndex].offset, configLayout[currentIndex].length, graph.Value.nodes[currentIndex].configurationMask) : 1;
                renderBoxLayout[currentIndex] = new OffsetInfo(count, l);
                count += renderBoxLayout[currentIndex].length;
            }
            return count;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Render(NativeArray<UIVertexData> vertices, UIContext* context, MeshData meshData, BlobAssetReference<UIGraph> graph, NativeArray<OffsetInfo> configLayout, NativeArray<OffsetInfo> renderBoxLayout, NativeArray<UIPassState> stateLayout, NativeArray<SubMeshInfo> subMeshes) {
            if (graph.Value.nodes.Length <= 0)
                return;
            var indices = meshData.GetIndexData<ushort>();
            int submeshIndex = 0;
            int renderIndex = 0;
            float4 bounds = float4.zero;
            RenderSubMesh0((UIVertexData*)vertices.GetUnsafePtr(), context, indices, graph, configLayout, renderBoxLayout, stateLayout, subMeshes, ref submeshIndex, ref renderIndex, ref bounds);
            meshData.subMeshCount = subMeshes.Length + 1;
            float4 totalBounds  = bounds;
            int submesh0RenderIndexCount = renderIndex;
            for (int i = 0; i < subMeshes.Length; i++) {
                //while (submeshes.IsCreated && !submeshes.IsEmpty()) {
                SubMeshInfo current = subMeshes[i];
                //var current = UnsafeUtility.ReadArrayElement<SubMeshInfo>(submeshes,i);
                var initialRenderIndex = renderIndex;
                graph.Value.nodes[current.nodeIndex].pass.Invoke(
                    (byte)UIPassType.Render,
                    (IntPtr)graph.Value.initialConfiguration.GetUnsafePtr(),
                    (IntPtr)configLayout.GetUnsafePtr(),
                    configLayout[current.nodeIndex].offset,
                    configLayout[current.nodeIndex].length,
                    graph.Value.nodes[current.nodeIndex].configurationMask,
                    (IntPtr)stateLayout.GetUnsafePtr(),
                    (int*)graph.Value.nodes[current.nodeIndex].children.GetUnsafePtr(),
                    current.nodeIndex,
                    -1,
                    graph.Value.nodes[current.nodeIndex].children.Length,
                    (IntPtr)(UIVertexData*)vertices.GetUnsafePtr(),
                    renderIndex * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                    (IntPtr)context
                );
                bounds = float4.zero;
                for (int j = 0; j < renderBoxLayout[current.nodeIndex].length; j++) {
                    indices[(renderIndex + j) * 6] = (ushort)((renderIndex + j) * 4);
                    indices[((renderIndex + j) * 6) + 1] = (ushort)(((renderIndex + j) * 4) + 2);
                    indices[((renderIndex + j) * 6) + 2] = (ushort)(((renderIndex + j) * 4) + 1);
                    indices[((renderIndex + j) * 6) + 3] = (ushort)(((renderIndex + j) * 4) + 2);
                    indices[((renderIndex + j) * 6) + 4] = (ushort)(((renderIndex + j) * 4) + 3);
                    indices[((renderIndex + j) * 6) + 5] = (ushort)(((renderIndex + j) * 4) + 1);
                    UpdateBounds(indices, (UIVertexData*)vertices.GetUnsafePtr(), (renderIndex + j) * 4, ref bounds);
                }
                renderIndex += renderBoxLayout[current.nodeIndex].length;
                RenderMesh(current.nodeIndex, (UIVertexData*)vertices.GetUnsafePtr(), context, indices, graph, configLayout, renderBoxLayout, stateLayout, subMeshes, true, false, false, ref submeshIndex, ref renderIndex, ref bounds);
                var size = new float3(math.abs(bounds.z - bounds.x), math.abs(bounds.y - bounds.w), 0f);
                totalBounds = new float4(math.min(totalBounds.x, bounds.x), math.min(totalBounds.y, bounds.y), math.max(totalBounds.z, bounds.z), math.max(totalBounds.w, bounds.w));
                current.meshIndexStart = initialRenderIndex * 6;
                current.meshIndexCount = (renderIndex - initialRenderIndex) * 6;
                current.bounds = bounds;
                subMeshes[i] = current;
                for (int j = 0; j < (renderIndex - initialRenderIndex); j++) {
                    for (int k = 0; k < 4; k++) {
                        UIVertexData vertex = vertices[(initialRenderIndex + j) * 4 + k];
                        vertex.position.z -= 0.001f * (i + 1);
                        vertices[(initialRenderIndex + j) * 4 + k] = vertex;
                    }
                }
                /*                 meshData.SetSubMesh(meshData.subMeshCount - (i + 1), new UnityEngine.Rendering.SubMeshDescriptor(current.meshIndexStart, current.meshIndexCount)
                                {
                                    bounds = new Bounds(new float3(bounds.x + (size.x / 2f), bounds.y + (size.y / 2f), 0f) - adjust, size),
                                    firstVertex = (current.meshIndexStart / 6) * 4,
                                    vertexCount = (current.meshIndexCount / 6) * 4
                                }
                                , MeshUpdateFlags.DontValidateIndices); */

                /*                 meshData.SetSubMesh(i + 1, new UnityEngine.Rendering.SubMeshDescriptor(current.meshIndexStart, current.meshIndexCount)
                                {
                                    bounds = new Bounds(new float3(bounds.x + (math.abs(bounds.z - bounds.x) / 2f), bounds.y + (math.abs(bounds.y - bounds.w) / 2f), 0f), new float3(math.abs(bounds.z - bounds.x), math.abs(bounds.y - bounds.w), 0f))
                                }); */
            }
            //meshData.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, indices.Length));
            //Center Mesh
            float3 totalSize = new float3(math.abs(totalBounds.z - totalBounds.x), math.abs(totalBounds.y - totalBounds.w), 0f);
            var adjust = new float3(totalSize.x / 2f, totalSize.y / 2f, 0);
            for (int i = 0; i < vertices.Length; i++) {
                UIVertexData vertex = vertices[i];
                vertex.position -= adjust;
                vertices[i] = vertex;
            }
            for (int i = 0; i < subMeshes.Length; i++) {
                SubMeshInfo current = subMeshes[i];
                var size = new float3(math.abs(current.bounds.z - current.bounds.x), math.abs(current.bounds.y - current.bounds.w), 0.00001f);
                meshData.SetSubMesh(meshData.subMeshCount - (i + 1), new UnityEngine.Rendering.SubMeshDescriptor(current.meshIndexStart, current.meshIndexCount)
                {

                    //bounds = new Bounds(new float3(current.bounds.x + (size.x / 2f), bounds.y + (size.y / 2f), 0f) - adjust, size),
                    bounds = new Bounds(float3.zero, size),
                    firstVertex = (current.meshIndexStart / 6) * 4,
                    vertexCount = (current.meshIndexCount / 6) * 4
                }
                //,MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices
                );

            }
            meshData.SetSubMesh(0, new UnityEngine.Rendering.SubMeshDescriptor(0, submesh0RenderIndexCount * 6)
            {

                bounds = new Bounds(float3.zero, new float3(totalSize.x,totalSize.y,0.00001f)),
                firstVertex = 0,
                vertexCount = submesh0RenderIndexCount * 4
            }
            //,MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices
            );
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderSubMesh0(
            UIVertexData* vertexPtr,
            UIContext* context,
            NativeArray<ushort> indices,
            BlobAssetReference<UIGraph> graph,
            NativeArray<OffsetInfo> configLayout,
            NativeArray<OffsetInfo> renderBoxLayout,
            NativeArray<UIPassState> stateLayout,
            NativeArray<SubMeshInfo> subMeshes,
            ref int currentSubmesh,
            ref int renderIndex,
            ref float4 bounds
            ) {
            const int currentIndex = 0;
            var box = stateLayout[currentIndex];
            box.globalBox = box.localBox;
            stateLayout[currentIndex] = box;
            if (UIConfigLayout.HasName(graph.Value.nodes[currentIndex].configurationMask, ((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + configLayout[currentIndex].offset)) {
                subMeshes[currentSubmesh] = new SubMeshInfo(++currentSubmesh, currentIndex);
                RenderMesh(currentIndex, vertexPtr, context, indices, graph, configLayout, renderBoxLayout, stateLayout, subMeshes, false, true, true, ref currentSubmesh, ref renderIndex, ref bounds);
            }
            else {

                graph.Value.nodes[currentIndex].pass.Invoke(
                    (byte)UIPassType.Render,
                    (IntPtr)graph.Value.initialConfiguration.GetUnsafePtr(),
                    (IntPtr)configLayout.GetUnsafePtr(),
                    configLayout[currentIndex].offset,
                    configLayout[currentIndex].length,
                    graph.Value.nodes[currentIndex].configurationMask,
                    (IntPtr)stateLayout.GetUnsafePtr(),
                    (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                    currentIndex,
                    -1,
                    graph.Value.nodes[currentIndex].children.Length,
                    (IntPtr)vertexPtr,
                    renderIndex * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                    (IntPtr)context
                );
                for (int j = 0; j < renderBoxLayout[currentIndex].length; j++) {
                    indices[(renderIndex + j) * 6] = (ushort)((renderIndex + j) * 4);
                    indices[((renderIndex + j) * 6) + 1] = (ushort)(((renderIndex + j) * 4) + 2);
                    indices[((renderIndex + j) * 6) + 2] = (ushort)(((renderIndex + j) * 4) + 1);
                    indices[((renderIndex + j) * 6) + 3] = (ushort)(((renderIndex + j) * 4) + 2);
                    indices[((renderIndex + j) * 6) + 4] = (ushort)(((renderIndex + j) * 4) + 3);
                    indices[((renderIndex + j) * 6) + 5] = (ushort)(((renderIndex + j) * 4) + 1);
                    UpdateBounds(indices, vertexPtr, (renderIndex + j) * 4, ref bounds);
                }
                renderIndex += renderBoxLayout[currentIndex].length;

                RenderMesh(currentIndex, vertexPtr, context, indices, graph, configLayout, renderBoxLayout, stateLayout, subMeshes, true, true, true, ref currentSubmesh, ref renderIndex, ref bounds);

            }

        }
        private void UpdateBounds(NativeArray<ushort> indices, UIVertexData* vertexPtr, int renderIndex, ref float4 bounds) {
            var topLeft = UnsafeUtility.ReadArrayElement<UIVertexData>(vertexPtr, renderIndex);
            var bottomRight = UnsafeUtility.ReadArrayElement<UIVertexData>(vertexPtr, renderIndex + 3);
            bounds = new float4(math.min(topLeft.position.x, bounds.x), math.min(topLeft.position.y, bounds.y), math.max(bottomRight.position.x, bounds.z), math.max(bottomRight.position.y, bounds.w));

        }
        private void RenderMesh(
            int index,
            UIVertexData* vertexPtr,
            UIContext* context,
            NativeArray<ushort> indices,
            BlobAssetReference<UIGraph> graph,
            NativeArray<OffsetInfo> configLayout,
            NativeArray<OffsetInfo> renderBoxLayout,
            NativeArray<UIPassState> stateLayout,
            NativeArray<SubMeshInfo> subMeshes,
            bool renderNow,
            bool updateSubmeshCount,
            bool accumulate,
            ref int currentSubmesh,
            ref int renderIndex,
            ref float4 bounds
            ) {
            for (int i = 0; i < graph.Value.nodes[index].children.Length; i++) {
                var currentIndex = graph.Value.nodes[index].children[i];
                if (accumulate) {
                    var box = stateLayout[currentIndex];
                    box.globalBox = stateLayout[index].globalBox + box.localBox;
                    stateLayout[currentIndex] = box;
                }
                if (UIConfigLayout.HasName(graph.Value.nodes[currentIndex].configurationMask, ((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + configLayout[currentIndex].offset)) {
                    if (updateSubmeshCount) {
                        subMeshes[currentSubmesh] = new SubMeshInfo(++currentSubmesh, currentIndex);
                    }
                    RenderMesh(currentIndex, vertexPtr, context, indices, graph, configLayout, renderBoxLayout, stateLayout, subMeshes, false, updateSubmeshCount, accumulate, ref currentSubmesh, ref renderIndex, ref bounds);
                }
                else if (renderNow) {
                    graph.Value.nodes[currentIndex].pass.Invoke(
                        (byte)UIPassType.Render,
                        (IntPtr)graph.Value.initialConfiguration.GetUnsafePtr(),
                        (IntPtr)configLayout.GetUnsafePtr(),
                        configLayout[currentIndex].offset,
                        configLayout[currentIndex].length,
                        graph.Value.nodes[currentIndex].configurationMask,
                        (IntPtr)stateLayout.GetUnsafePtr(),
                        (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                        currentIndex,
                        -1,
                        graph.Value.nodes[currentIndex].children.Length,
                        (IntPtr)vertexPtr,
                        renderBoxLayout[currentIndex].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                        (IntPtr)context
                    );
                    for (int j = 0; j < renderBoxLayout[currentIndex].length; j++) {
                        indices[(renderIndex + j) * 6] = (ushort)((renderIndex + j) * 4);
                        indices[((renderIndex + j) * 6) + 1] = (ushort)(((renderIndex + j) * 4) + 2);
                        indices[((renderIndex + j) * 6) + 2] = (ushort)(((renderIndex + j) * 4) + 1);
                        indices[((renderIndex + j) * 6) + 3] = (ushort)(((renderIndex + j) * 4) + 2);
                        indices[((renderIndex + j) * 6) + 4] = (ushort)(((renderIndex + j) * 4) + 3);
                        indices[((renderIndex + j) * 6) + 5] = (ushort)(((renderIndex + j) * 4) + 1);
                        UpdateBounds(indices, vertexPtr, (renderIndex + j) * 4, ref bounds);
                    }
                    renderIndex += renderBoxLayout[currentIndex].length;

                    RenderMesh(currentIndex, vertexPtr, context, indices, graph, configLayout, renderBoxLayout, stateLayout, subMeshes, true, updateSubmeshCount, accumulate, ref currentSubmesh, ref renderIndex, ref bounds);

                }
                else {
                    RenderMesh(currentIndex, vertexPtr, context, indices, graph, configLayout, renderBoxLayout, stateLayout, subMeshes, false, updateSubmeshCount, accumulate, ref currentSubmesh, ref renderIndex, ref bounds);
                }

            }
        }
        private void Layout(int currentIndex, BlobAssetReference<UIGraph> graph, NativeArray<OffsetInfo> configLayout, NativeArray<OffsetInfo> renderBoxLayout, NativeArray<UIPassState> stateLayout, UIContext* context, UIVertexData* vertexPtr) {
            var configSource = ((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + configLayout[currentIndex].offset;
            //var config = (UIConfig*)((((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + configLayout[currentIndex].offset).ToPointer());
            var state = (UIPassState*)(((IntPtr)stateLayout.GetUnsafePtr()) + (UnsafeUtility.SizeOf<UIPassState>() * currentIndex)).ToPointer();
            float4 padding = float4.zero;
            if (UIConfigLayout.TryGetConfig(graph.Value.nodes[currentIndex].configurationMask, UIConfigLayout.BoxConfig, configSource, out IntPtr boxConfig)) {
                BoxConfig* boxConfigPtr = ((BoxConfig*)boxConfig.ToPointer());
                state->localBox += boxConfigPtr->margin.Normalize(*context);
                padding = boxConfigPtr->padding.Normalize(*context);
            }
            for (int index = 0; index < graph.Value.nodes[currentIndex].children.Length; index++) {
                graph.Value.nodes[currentIndex].pass.Invoke(
                    (byte)UIPassType.Constrain,
                    (IntPtr)graph.Value.initialConfiguration.GetUnsafePtr(),
                    (IntPtr)configLayout.GetUnsafePtr(),
                    configLayout[currentIndex].offset,
                    configLayout[currentIndex].length,
                    graph.Value.nodes[currentIndex].configurationMask,
                    (IntPtr)stateLayout.GetUnsafePtr(),
                    (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                    currentIndex,
                    index,
                    graph.Value.nodes[currentIndex].children.Length,
                    (IntPtr)vertexPtr,
                    renderBoxLayout[currentIndex].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                    (IntPtr)context
                );
                Layout(graph.Value.nodes[currentIndex].children[index], graph, configLayout, renderBoxLayout, stateLayout, context, vertexPtr);

            }
            graph.Value.nodes[currentIndex].pass.Invoke(
                (byte)UIPassType.Size,
                (IntPtr)graph.Value.initialConfiguration.GetUnsafePtr(),
                (IntPtr)configLayout.GetUnsafePtr(),
                configLayout[currentIndex].offset,
                configLayout[currentIndex].length,
                graph.Value.nodes[currentIndex].configurationMask,
                (IntPtr)stateLayout.GetUnsafePtr(),
                (int*)graph.Value.nodes[currentIndex].children.GetUnsafePtr(),
                currentIndex,
                -1,
                graph.Value.nodes[currentIndex].children.Length,
                (IntPtr)vertexPtr,
                renderBoxLayout[currentIndex].offset * UnsafeUtility.SizeOf<UIVertexData>() * 4,
                (IntPtr)context
            );
            // + config->padding.left.Normalize(*context) + config->padding.right.Normalize(*context)
            // + config->padding.top.Normalize(*context) + config->padding.bottom.Normalize(*context)
            if (UIConfigLayout.TryGetConfig(graph.Value.nodes[currentIndex].configurationMask, UIConfigLayout.SizeConfig, configSource, out IntPtr sizeConfig)) {
                var sizeConfigPtr = ((SizeConfig*)sizeConfig.ToPointer());
                state->size = new float2(
                    math.clamp(state->size.x, sizeConfigPtr->minWidth.Normalize(*context), sizeConfigPtr->maxWidth.Normalize(*context)),
                    math.clamp(state->size.y, sizeConfigPtr->minHeight.Normalize(*context), sizeConfigPtr->maxHeight.Normalize(*context)));
            }


        }



        public struct ConfigHandle {
            public void* value;
        }
        private struct SubMeshInfo {
            public int subMesh;
            public int nodeIndex;
            public int meshIndexStart;
            public int meshIndexCount;
            public float4 bounds;

            public SubMeshInfo(int subMesh, int index) {
                this.subMesh = subMesh;
                this.nodeIndex = index;
                meshIndexStart = 0;
                meshIndexCount = 0;
                bounds = float4.zero;
            }
        }


    }
    public struct OffsetInfo {
        public int offset;
        public int length;

        public OffsetInfo(int offset, int length) {
            this.offset = offset;
            this.length = length;
        }
    }

}