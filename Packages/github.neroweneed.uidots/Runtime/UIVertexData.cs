using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace NeroWeNeed.UIDots {
    public struct UIVertexData {

        public static NativeArray<VertexAttributeDescriptor> AllocateVertexDescriptor(Allocator allocator = Allocator.TempJob) {
            var array = new NativeArray<VertexAttributeDescriptor>(7, allocator);
            array[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            array[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            array[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4);
            array[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
            array[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 3);
            array[5] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 3);
            array[6] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.UNorm8, 4);
            return array;
        }

        /// <summary>
        /// 2D Coordinates (X,Y)
        /// </summary>
        public float3 position;

        public float3 normals;

        /// /// <summary>
        /// Color
        /// </summary>
        public Color32 foregroundColor;
        /// <summary>
        /// 2D Coordinates + Texture Stack index (X,Y,Texture stack index)
        /// </summary>
        public float2 background;
        public float3 foreground;
        /// <summary>
        /// Border box stored Angle, Distance (Angle, Distance,Radius)
        /// </summary>
        public float3 border;
        /// <summary>
        /// Border Color
        /// </summary>
        public Color32 borderColor;
    }
}