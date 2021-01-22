using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    public unsafe static class UIDataExtensions {
        /// <summary>
        /// The length of the UI Graph, excluding the total length field.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetLength(this UIGraphData self) => self.IsCreated ? *(ulong*)self.value.ToPointer() : 0UL;
        /// <summary>
        /// The total nodes present in the UI Graph.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNodeCount(this UIGraphData self) => self.IsCreated ? *(int*)(self.value + sizeof(ulong)).ToPointer() : 0;
        /// <summary>
        /// Length of a given node in the UI Graph, excluding the length field.
        /// </summary>
        /// <param name="index">Index of node</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetNodeLength(this UIGraphData self, int index) {
            return *(int*)GetNodePointer(self, index).ToPointer();
        }
        /// <summary>
        /// Header of a given node in the UI Graph.
        /// </summary>
        /// <param name="index">Index of node</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetNodeHeader(this UIGraphData self, int index, ref HeaderConfig result) {
            result = UnsafeUtility.AsRef<HeaderConfig>((GetNodePointer(self, index) + sizeof(int)).ToPointer());
        }
        /// <inheritdoc cref="GetNodeHeader" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HeaderConfig GetNodeHeader(this UIGraphData self, int index) {
            HeaderConfig result = default;
            GetNodeHeader(self, index, ref result);
            return result;
        }
        /// <summary>
        /// Pointer to a given node in the UI Graph. This pointer includes the size field.
        /// </summary>
        /// <param name="index">Index of node</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetNodePointer(this UIGraphData self, int index) {
            var offset = sizeof(ulong) + sizeof(int);
            for (int currentIndex = 0; currentIndex < index; currentIndex++) {
                offset += *(int*)(self.value + offset).ToPointer() + sizeof(int);
            }
            return self.value + offset;
        }
        /// <summary>
        /// Configuration Mask of a given node. The configuration mask is used to determine what Config Blocks are present in the node.
        /// </summary>
        /// <param name="index">Index of node</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetNodeConfigurationMask(this UIGraphData self, int index) {
            HeaderConfig result = default;
            GetNodeHeader(self, index, ref result);
            return result.configurationMask;
        }
        /// <summary>
        /// Gets the start location of the Config Blocks present in this node.
        /// </summary>
        /// <param name="index">Index of node</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetNodeConfigPointer(this UIGraphData self, int index) {
            HeaderConfig* header = (HeaderConfig*)(GetNodePointer(self, index) + sizeof(int)).ToPointer();
            var childCount = header->childCount;
            return ((IntPtr)header) + UnsafeUtility.SizeOf<HeaderConfig>() + (sizeof(int) * childCount);
        }
        /// <summary>
        /// Returns the start location of a given Config Block.
        /// <seealso cref="UIConfigLayoutTable"/>
        /// </summary>
        /// <param name="index">Index of node</param>
        /// <param name="config">Config block id</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetConfigBlock(this UIGraphData self, int index, byte config, out IntPtr configBlockStart) {
            var ptr = GetNodePointer(self, index);
            HeaderConfig* header = (HeaderConfig*)(ptr + sizeof(int)).ToPointer();
            var childCount = header->childCount;
            var offset = UIConfigUtility.GetOffset(header->configurationMask, config);
            configBlockStart = ptr + (sizeof(int) * (header->childCount + 1)) + UnsafeUtility.SizeOf<HeaderConfig>();
            return ((IntPtr)header) + UnsafeUtility.SizeOf<HeaderConfig>() + (sizeof(int) * childCount) + offset;
        }
        /// <summary>
        /// Returns the start location of a given Config Block.
        /// <seealso cref="UIConfigLayoutTable"/>
        /// </summary>
        /// <param name="index">Index of node</param>
        /// <param name="config">Config block id</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetConfigBlock(this UIGraphData self, int index, byte config) {
            HeaderConfig* header = (HeaderConfig*)(GetNodePointer(self, index) + sizeof(int)).ToPointer();
            var childCount = header->childCount;
            var offset = UIConfigUtility.GetOffset(header->configurationMask, config);
            return ((IntPtr)header) + UnsafeUtility.SizeOf<HeaderConfig>() + (sizeof(int) * childCount) + offset;
        }
        /// <summary>
        /// Returns the start location of a given Config Block.
        /// </summary>
        /// <param name="index">Index of node</param>
        /// <param name="config">Config Block ID. Config Block IDs can be found in <see cref="UIConfigLayoutTable"/></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetConfigBlock(this UIGraphData self, int index, byte config, out IntPtr result, out IntPtr nodeStart) {
            var ptr = GetNodePointer(self, index);
            nodeStart = ptr + sizeof(int);
            HeaderConfig* header = (HeaderConfig*)nodeStart.ToPointer();
            var childCount = header->childCount;
            var offset = UIConfigUtility.GetOffset(header->configurationMask, config);
            if (offset < 0) {
                result = IntPtr.Zero;
                return false;
            }
            else {
                result = ((IntPtr)header) + UnsafeUtility.SizeOf<HeaderConfig>() + (sizeof(int) * childCount) + offset;
                return true;
            }
        }
        /// <summary>
        /// Returns the start location of a given Config Block.
        /// </summary>
        /// <param name="index">Index of node</param>
        /// <param name="config">Config Block ID. Config Block IDs can be found in <see cref="UIConfigLayoutTable"/></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetConfigBlock(this UIGraphData self, int index, byte config, out IntPtr result) {
            HeaderConfig* header = (HeaderConfig*)(GetNodePointer(self, index) + sizeof(int)).ToPointer();
            var childCount = header->childCount;
            var offset = UIConfigUtility.GetOffset(header->configurationMask, config);
            if (offset < 0) {
                result = IntPtr.Zero;
                return false;
            }
            else {
                result = ((IntPtr)header) + UnsafeUtility.SizeOf<HeaderConfig>() + (sizeof(int) * childCount) + offset;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GraphInfo GetGraphInfo(this UIGraphData graph, out NativeArray<NodeInfo> configLayout, Allocator allocator) {
            var offset = sizeof(ulong) + sizeof(int);

            configLayout = new NativeArray<NodeInfo>(graph.GetNodeCount(), allocator);
            var graphInfo = new GraphInfo();
            for (int currentIndex = 0; currentIndex < graph.GetNodeCount(); currentIndex++) {
                //var size = UnsafeUtility.AsRef<int>((((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + offset).ToPointer());
                var size = *(int*)(graph.value + offset).ToPointer();
                var header = (HeaderConfig*)(graph.value + offset + sizeof(int)).ToPointer();
                //var size = graph.GetNodeLength(currentIndex);
                if (header->IsDedicatedNode) {
                    graphInfo.subMeshCount++;
                }
                offset += UnsafeUtility.SizeOf<int>();
                NodeInfo info = new NodeInfo
                {
                    configurationMask = header->configurationMask,
                    nodeOffset = offset,
                    childrenOffset = offset + UnsafeUtility.SizeOf<HeaderConfig>(),
                    configOffset = offset + UnsafeUtility.SizeOf<HeaderConfig>() + (sizeof(int) * header->childCount),
                    length = size,
                    index = currentIndex
                };
                info.renderBoxCount = header->renderBoxCounter.IsCreated ? header->renderBoxCounter.Invoke(graph.value, (NodeInfo*)UnsafeUtility.AddressOf(ref info)) : 1;
                configLayout[currentIndex] = info;
                graphInfo.renderBoxCount += info.renderBoxCount;
                offset += size;
            }
            return graphInfo;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GraphInfo GetGraphInfo(this UIGraphData graph) {
            var offset = sizeof(ulong) + sizeof(int);
            var graphInfo = new GraphInfo();
            for (int currentIndex = 0; currentIndex < graph.GetNodeCount(); currentIndex++) {
                //var size = UnsafeUtility.AsRef<int>((((IntPtr)graph.Value.initialConfiguration.GetUnsafePtr()) + offset).ToPointer());
                var size = *(int*)(graph.value + offset).ToPointer();
                var header = (HeaderConfig*)(graph.value + offset + sizeof(int)).ToPointer();
                //var size = graph.GetNodeLength(currentIndex);
                if (header->IsDedicatedNode) {
                    graphInfo.subMeshCount++;
                }
                offset += UnsafeUtility.SizeOf<int>();
                NodeInfo info = new NodeInfo
                {
                    configurationMask = header->configurationMask,
                    nodeOffset = offset,
                    childrenOffset = offset + UnsafeUtility.SizeOf<HeaderConfig>(),
                    configOffset = offset + UnsafeUtility.SizeOf<HeaderConfig>() + (sizeof(int) * header->childCount),
                    length = size,
                    index = currentIndex
                };
                graphInfo.renderBoxCount += header->renderBoxCounter.IsCreated ? header->renderBoxCounter.Invoke(graph.value, (NodeInfo*)UnsafeUtility.AddressOf(ref info)) : 1;
                offset += size;
            }
            return graphInfo;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountRenderBoxes(this UIGraphData graph, NativeArray<NodeInfo> configLayout) {
            int count = 0;
            for (int currentIndex = 0; currentIndex < configLayout.Length; currentIndex++) {
                var info = configLayout[currentIndex];
                var header = (HeaderConfig*)(graph.value + info.nodeOffset);
                count += header->renderBoxCounter.IsCreated ? header->renderBoxCounter.Invoke(graph.value, (NodeInfo*)UnsafeUtility.AddressOf(ref info)) : 1;
            }
            return count;
        }
        public static int GetFirstSelectableIndex(this UIGraphData graph) {
            if (!graph.IsCreated)
                return -1;
            //Index, priority
            ValueTuple<int, int> selectableIndex = (-1, int.MinValue);
            for (int currentIndex = 0; currentIndex < graph.GetNodeCount(); currentIndex++) {
                if (graph.TryGetConfigBlock(currentIndex, UIConfigLayoutTable.SelectableConfig, out IntPtr block)) {
                    SelectableConfig* selectableConfig = (SelectableConfig*)block;
                    if (selectableConfig->onSelect.IsCreated && selectableIndex.Item2.CompareTo(selectableConfig->priority) < 0) {
                        selectableIndex = (currentIndex, selectableConfig->priority);
                    }
                }
            }
            return selectableIndex.Item1;
        }

        public static bool IsDedicatedNode(this UIGraphData self, int index) => GetNodeHeader(self, index).IsDedicatedNode;

        [BurstDiscard]
        public static string GetNodeName(this UIGraphData self, int index) {
            if (TryGetConfigBlock(self, index, UIConfigLayoutTable.NameConfig, out IntPtr block, out IntPtr header)) {
                return ((NameConfig*)block.ToPointer())->name.ToString(header.ToPointer());
            }
            else {
                return null;
            }
        }
    }
    public struct GraphInfo {
        public int MeshCount { get => subMeshCount + 1; }
        public int subMeshCount;
        public int renderBoxCount;
    }
    public struct NodeInfo {
        public ulong configurationMask;
        public int index;
        public int nodeOffset;
        public int childrenOffset;
        public int configOffset;
        public int renderBoxCount;
        public int length;
    }
    public struct OffsetInfo {
        public ulong configurationMask;
        public int offset;
        public int length;

        public OffsetInfo(int offset, int length, ulong configurationMask) {
            this.configurationMask = configurationMask;
            this.offset = offset;
            this.length = length;
        }
    }
    public struct DedicatedNodeInfo {
        public int graphIndex;
        public int nodeIndex;
        public UIDots.NodeInfo nodeInfo;
        public int submesh;

        public DedicatedNodeInfo(int graphIndex, int nodeIndex, UIDots.NodeInfo nodeInfo, int submesh) {
            this.graphIndex = graphIndex;
            this.nodeIndex = nodeIndex;
            this.nodeInfo = nodeInfo;
            this.submesh = submesh;
        }
    }
}