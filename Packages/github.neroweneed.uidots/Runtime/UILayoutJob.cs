
using System;
using System.Runtime.CompilerServices;
using NeroWeNeed.UIDots;
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
    public unsafe struct UILayoutJob : IJobParallelFor {
        [NativeDisableUnsafePtrRestriction]
        public BlobAssetReference<CompiledUISchema> schema;

        [ReadOnly]
        //[DeallocateOnJobCompletion]
        [NativeDisableUnsafePtrRestriction]
        public NativeArray<UIGraphData> graphs;
        [NativeDisableUnsafePtrRestriction]
        public NativeArray<UIContextData> contexts;
        [NativeDisableContainerSafetyRestriction]
        [NativeDisableUnsafePtrRestriction]
        [NativeDisableParallelForRestriction]
        public MeshDataArray meshDataArray;
        public void Execute(int index) {
            Execute(graphs[index], meshDataArray[index], index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(UIGraphData graph, MeshData meshData, int index) {

            if (graph.GetNodeCount() > 0) {
                var states = new NativeArray<UIPassState>(graph.GetNodeCount(), Allocator.Temp);
                var initial = UIPassState.Null;
                UnsafeUtility.MemCpyReplicate(states.GetUnsafePtr(), UnsafeUtility.AddressOf(ref initial), UnsafeUtility.SizeOf<UIPassState>(), states.Length);
                var graphInfo = graph.GetGraphInfo(schema, out NativeArray<NodeInfo> layout, Allocator.Temp);
                InitMeshData(ref meshData, graphInfo);
                NativeArray<UIVertexData> vertices = meshData.GetVertexData<UIVertexData>();
                var contextPtr = (UIContextData*)((IntPtr)contexts.GetUnsafePtr() + (UnsafeUtility.SizeOf<UIContextData>() * index)).ToPointer();
                Layout(0, graph, layout, states, contextPtr);
                Render(vertices, contextPtr, ref meshData, graph, layout, graphInfo, states);
                states.Dispose();
                layout.Dispose();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitMeshData(ref MeshData meshData, GraphInfo graphInfo) {
            var parameters = UIVertexData.AllocateVertexDescriptor(Allocator.Temp);
            meshData.SetVertexBufferParams(graphInfo.renderBoxCount * 4, parameters);
            meshData.SetIndexBufferParams(graphInfo.renderBoxCount * 6, UnityEngine.Rendering.IndexFormat.UInt16);
            meshData.subMeshCount = graphInfo.MeshCount;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Render(NativeArray<UIVertexData> vertices, UIContextData* context, ref MeshData meshData, UIGraphData graph, NativeArray<NodeInfo> layout, GraphInfo graphInfo, NativeArray<UIPassState> stateLayout) {
            var indices = meshData.GetIndexData<ushort>();
            var subMeshes = new NativeArray<SubMeshInfo>(graphInfo.subMeshCount, Allocator.Temp);
            int submeshIndex = 0;
            int renderIndex = 0;
            float4 bounds = float4.zero;
            RenderMesh(0, -1, 0, vertices, context, indices, graph, graphInfo, layout, stateLayout, subMeshes, true, true, true, ref submeshIndex, ref renderIndex, ref bounds);
            float4 totalBounds = bounds;
            int submesh0RenderIndexCount = renderIndex;
            for (int i = 0; i < subMeshes.Length; i++) {
                SubMeshInfo current = subMeshes[i];
                var initialRenderIndex = renderIndex;
                RenderMesh(current.nodeIndex, -1, current.nodeIndex, vertices, context, indices, graph, graphInfo, layout, stateLayout, subMeshes, true, false, false, ref submeshIndex, ref renderIndex, ref bounds);
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
            }
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
                bounds = new Bounds(float3.zero, new float3(totalSize.x, totalSize.y, 0.00001f)),
                firstVertex = 0,
                vertexCount = submesh0RenderIndexCount * 4
            }
            //,MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices
            );
            subMeshes.Dispose();
        }
        private void RenderMesh(
            int startIndex,
            int parentIndex,
            int currentIndex,
            NativeArray<UIVertexData> vertexData,
            UIContextData* context,
            NativeArray<ushort> indices,
            UIGraphData graph,
            GraphInfo graphInfo,
            NativeArray<NodeInfo> nodeInfo,
            NativeArray<UIPassState> stateLayout,
            NativeArray<SubMeshInfo> subMeshes,
            bool renderNow,
            bool updateSubmeshCount,
            bool accumulate,
            ref int subMeshIndex,
            ref int renderIndex,
            ref float4 bounds

            ) {
            var info = nodeInfo[currentIndex];
            HeaderConfig* headerConfig = (HeaderConfig*)(graph.value + info.nodeOffset).ToPointer();
            var state = stateLayout[currentIndex];
            if (accumulate) {
                state.globalBox += state.localBox;
                if (parentIndex >= 0) {
                    state.globalBox += stateLayout[parentIndex].inner;
                }
                stateLayout[currentIndex] = state;
            }
            if (headerConfig->IsDedicatedNode) {
                if (updateSubmeshCount) {
                    subMeshes[subMeshIndex] = new SubMeshInfo(++subMeshIndex, currentIndex);
                }
                renderNow = currentIndex == startIndex;
            }
            if (renderNow) {
                bool display = true;
                bool visible = true;
                if (graph.TryGetConfigBlock(currentIndex, UIConfigLayoutTable.DisplayConfig, out IntPtr displayConfig)) {
                    var dc = ((DisplayConfig*)displayConfig.ToPointer());
                    display = dc->display == VisibilityStyle.Visible;
                    visible = dc->visible == VisibilityStyle.Visible;
                }
                if (display) {
                    FunctionPointer<UIRenderPass> render = headerConfig->schemaIndex >= 0 ? schema.Value.elements[headerConfig->schemaIndex].render : default;
                    if (render.IsCreated)
                        render.Invoke(
                            graph.value,
                            (NodeInfo*)UnsafeUtility.AddressOf(ref info),
                            (UIPassState*)UnsafeUtility.AddressOf(ref state),
                            (UIVertexData*)(((IntPtr)vertexData.GetUnsafePtr()) + (renderIndex * UnsafeUtility.SizeOf<UIVertexData>() * 4)).ToPointer(),
                            context
                        );
                    for (int j = 0; j < info.renderBoxCount; j++) {
                        indices[(renderIndex + j) * 6] = (ushort)((renderIndex + j) * 4);
                        indices[((renderIndex + j) * 6) + 1] = (ushort)(((renderIndex + j) * 4) + 2);
                        indices[((renderIndex + j) * 6) + 2] = (ushort)(((renderIndex + j) * 4) + 1);
                        indices[((renderIndex + j) * 6) + 3] = (ushort)(((renderIndex + j) * 4) + 2);
                        indices[((renderIndex + j) * 6) + 4] = (ushort)(((renderIndex + j) * 4) + 3);
                        indices[((renderIndex + j) * 6) + 5] = (ushort)(((renderIndex + j) * 4) + 1);
                        UpdateBounds(vertexData, (renderIndex + j) * 4, ref bounds);
                    }
                }
                if (!display || !visible) {
                    UnsafeUtility.MemClear((((IntPtr)vertexData.GetUnsafePtr()) + (renderIndex * UnsafeUtility.SizeOf<UIVertexData>() * 4)).ToPointer(), UnsafeUtility.SizeOf<UIVertexData>() * info.renderBoxCount * 4);
                    UnsafeUtility.MemClear((((IntPtr)indices.GetUnsafePtr()) + (renderIndex * UnsafeUtility.SizeOf<ushort>() * 6)).ToPointer(), UnsafeUtility.SizeOf<ushort>() * info.renderBoxCount * 6);
                }
                renderIndex += info.renderBoxCount;
            }
            for (int i = 0; i < headerConfig->childCount; i++) {
                RenderMesh(startIndex, currentIndex, UnsafeUtility.ReadArrayElement<int>((graph.value + info.childrenOffset).ToPointer(), i), vertexData, context, indices, graph, graphInfo, nodeInfo, stateLayout, subMeshes, renderNow, updateSubmeshCount, accumulate, ref subMeshIndex, ref renderIndex, ref bounds);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateBounds(NativeArray<UIVertexData> vertices, int renderIndex, ref float4 bounds) {
            var topLeft = vertices[renderIndex];
            var bottomRight = vertices[renderIndex + 3];
            bounds = new float4(math.min(topLeft.position.x, bounds.x), math.min(topLeft.position.y, bounds.y), math.max(bottomRight.position.x, bounds.z), math.max(bottomRight.position.y, bounds.w));
        }
        private void Layout(int index, UIGraphData graph, NativeArray<NodeInfo> configLayout, NativeArray<UIPassState> stateLayout, UIContextData* context) {
            if (!graph.TryGetConfigBlock(index, UIConfigLayoutTable.DisplayConfig, out IntPtr displayConfig) || (((DisplayConfig*)displayConfig.ToPointer())->display == VisibilityStyle.Visible)) {
                var nodeInfo = configLayout[index];
                var headerConfig = (HeaderConfig*)(graph.value + nodeInfo.nodeOffset).ToPointer();
                var configSource = graph.value + nodeInfo.configOffset;
                var state = (UIPassState*)(((IntPtr)stateLayout.GetUnsafePtr()) + (UnsafeUtility.SizeOf<UIPassState>() * index)).ToPointer();

                IntPtr boxConfig;
                bool hasBoxConfig = graph.TryGetConfigBlock(index, UIConfigLayoutTable.BoxModelConfig, out boxConfig);
                if (hasBoxConfig) {
                    BoxModelConfig* boxConfigPtr = (BoxModelConfig*)boxConfig.ToPointer();
                    state->localBox += boxConfigPtr->margin.Normalize(*context);
                    state->inner = boxConfigPtr->padding.Normalize(*context);

                }
                FunctionPointer<UILayoutPass> layout = headerConfig->schemaIndex >= 0 ? schema.Value.elements[headerConfig->schemaIndex].layout : default;
                for (int childIndex = 0; childIndex < headerConfig->childCount; childIndex++) {
                    if (layout.IsCreated)
                        layout.Invoke(
                            childIndex,
                            graph.value,
                            (NodeInfo*)UnsafeUtility.AddressOf(ref nodeInfo),
                            (IntPtr)stateLayout.GetUnsafePtr(),
                            context
                        );
                    Layout(UnsafeUtility.ReadArrayElement<int>((graph.value + nodeInfo.childrenOffset).ToPointer(), childIndex), graph, configLayout, stateLayout, context);
                }
                if (layout.IsCreated)
                    layout.Invoke(
                            -1,
                            graph.value,
                            (NodeInfo*)UnsafeUtility.AddressOf(ref nodeInfo),
                            (IntPtr)stateLayout.GetUnsafePtr(),
                            context
                        );
                if (graph.TryGetConfigBlock(index, UIConfigLayoutTable.SizeConfig, out IntPtr sizeConfig)) {
                    var sizeConfigPtr = ((SizeConfig*)sizeConfig.ToPointer());
                    state->size = new float2(
                        math.clamp(state->size.x, sizeConfigPtr->minWidth.Normalize(*context), sizeConfigPtr->maxWidth.Normalize(*context)),
                        math.clamp(state->size.y, sizeConfigPtr->minHeight.Normalize(*context), sizeConfigPtr->maxHeight.Normalize(*context)));
                }
                if (hasBoxConfig) {
                    state->size += new float2(state->inner.x + state->inner.z, state->inner.y + state->inner.w);
                }
            }
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
}