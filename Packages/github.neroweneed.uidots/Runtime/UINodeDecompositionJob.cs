
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace NeroWeNeed.UIDots {
    public struct UINodeDecompositionJob : IJobParallelFor {
        [ReadOnly]
        [NativeDisableUnsafePtrRestriction]
        public NativeArray<UIGraphData> graphs;
        [WriteOnly]
        public NativeArray<int> submeshCount;
        [WriteOnly]
        public NativeStream.Writer nodes;
        public void Execute(int index) {
            DecomposeHead(graphs[index], index);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void DecomposeHead(UIGraphData graph, int threadIndex) {
            var graphInfo = graph.GetGraphInfo(out NativeArray<NodeInfo> configLayout, Allocator.Temp);
            int currentSubmesh = 0;
            var dedicatedNodeInfo = new NativeArray<DedicatedNodeInfo>(graphInfo.subMeshCount, Allocator.Temp);
            Decompose(graph, threadIndex, 0, configLayout, ref currentSubmesh, dedicatedNodeInfo, graphInfo.subMeshCount+1);
            nodes.BeginForEachIndex(threadIndex);
            this.submeshCount[threadIndex] = graphInfo.subMeshCount;
            for (int i = 0; i < graphInfo.subMeshCount; i++) {
                nodes.Write(dedicatedNodeInfo[i]);
            }
            nodes.EndForEachIndex();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Decompose(UIGraphData graph, int threadIndex, int currentIndex, NativeArray<NodeInfo> nodeInfo, ref int currentSubmesh, NativeArray<DedicatedNodeInfo> nodes, int subMeshCount) {
            var ptr = graph.GetNodePointer(currentIndex) + sizeof(int);
            var header = (HeaderConfig*)ptr;
            if (header->IsDedicatedNode) {
                nodes[currentSubmesh] = new DedicatedNodeInfo(threadIndex, currentIndex, nodeInfo[currentIndex], subMeshCount - (++currentSubmesh));
            }
            var children = ptr + UnsafeUtility.SizeOf<HeaderConfig>();
            for (int i = 0; i < header->childCount; i++) {
                Decompose(graph, threadIndex, UnsafeUtility.ReadArrayElement<int>(children.ToPointer(), i), nodeInfo, ref currentSubmesh, nodes, subMeshCount);
            }
        }
    }
}