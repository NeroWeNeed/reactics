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
/// Delegate used for handling the layout pass phase.
/// </summary>
/// <param name="childIndex">Target child index. If this value is greater than or equal to 0, then this call is to be considered a constrain layout call and the index refers to the target child node. If this value is less than 0, then this call is to be considered a size layout call.</param>
/// <param name="configPtr">Root pointer of the graph</param>
/// <param name="nodeInfo">Current node info</param>
/// <param name="statePtr">Root pointer of the layout state</param>
/// <param name="context"></param>

    public unsafe delegate void UILayoutPass(
        int childIndex,
        IntPtr configPtr,
        NodeInfo* nodeInfo,
        IntPtr statePtr,
        UIContextData* context
    );
    /// <summary>
    /// Delegate used for handling the render pass phase.
    /// </summary>
    /// <param name="configPtr"></param>
    /// <param name="nodeInfo"></param>
    /// <param name="state"></param>
    /// <param name="vertexDataPtr"></param>
    /// <param name="context"></param>
    public unsafe delegate void UIRenderPass(
        IntPtr configPtr,
        NodeInfo* nodeInfo,
        UIPassState* state,
        UIVertexData* vertexData,
        UIContextData* context
    );
    /// <summary>
    /// Optional delegate for determining how many render boxes are needed for each element. If not set, element will use 1.
    /// </summary>
    /// <param name="configPtr"></param>
    public unsafe delegate int UIRenderBoxCounter(IntPtr configPtr, NodeInfo* nodeInfo);
    public struct UIPassState {
        public static readonly UIPassState Null = new UIPassState
        {
            widthConstraint = new float2(0, float.PositiveInfinity),
            heightConstraint = new float2(0, float.PositiveInfinity)
        };
        public float2 widthConstraint;
        public float2 heightConstraint;
        public float4 outer, inner;
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