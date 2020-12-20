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
    /// <summary>
    /// Pass Delegate
    /// </summary>
    /// <param name="configPtr"></param>
    /// <param name="statePtr"></param>
    /// <param name="vertexPtr"></param>
    /// <param name="type"></param>
    /// <param name="stateIndex">Represents the index in the State Array.</param>
    /// <param name="stateParentIndex">Represents the parent index.</param>
    /// <param name="layoutChildIndex">Represents the child being laid out. -1 during size pass</param>
    /// <param name="node"></param>

    public unsafe delegate void UIPass(
        byte type,
        IntPtr configPtr,
        IntPtr configOffsetLayoutPtr,
        int configOffset,
        int configLength,
        ulong configurationMask,
        IntPtr statePtr,
        int* stateChildren,
        int stateIndex,
        int stateChildLocalIndex,
        int stateChildCount,
        IntPtr vertexDataPtr,
        int vertexDataOffset,
        IntPtr context
    );
    /// <summary>
    /// Optional delegate for determining how many render boxes are needed for each element. If not set, element will use 1.
    /// </summary>
    /// <param name="configPtr"></param>
    /// <returns></returns>
    public unsafe delegate int UIRenderBoxHandler(IntPtr configPtr, int configOffset, int configLength,ulong configurationMask);
    public struct UIPassState {
        public static readonly UIPassState DEFAULT = new UIPassState
        {
            widthConstraint = new float2(0, float.PositiveInfinity),
            heightConstraint = new float2(0, float.PositiveInfinity)
        };
        public float2 widthConstraint;
        public float2 heightConstraint;
        public float4 globalBox;
        public float4 localBox;
        public float2 size;
    }
    /// <summary>
    /// Layout Passes are call going down the tree, size passes are called going up the tree. General Flow is as follows:
    /// Layout pass is called on container, passing index to indicate which child to layout.
    /// If child is a container, layout pass is called on child, passing an index to indicate which child to layout. This is repeated until a terminal node is reached.
    /// When terminal node is reached, size pass is called on node and the node goes back
    /// </summary>
    public enum UIPassType : byte {
        Unknown = 0, Constrain = 2, Size = 3, Render = 5
    }
}