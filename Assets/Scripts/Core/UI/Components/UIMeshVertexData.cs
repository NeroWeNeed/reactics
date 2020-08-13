using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Core.UI {

    public struct UIMeshVertexData : IBufferElementData {
        public float3 vertex;
        public float3 normal;
        public float2 uv;
        public UIMeshVertexData(float3 vertex) {
            this.vertex = vertex;
            this.normal = float3.zero;
            this.uv = float2.zero;
        }
        public UIMeshVertexData(float x, float y, float z) : this(new float3(x, y, z)) { }

        public UIMeshVertexData(float3 vertex, float3 normal, float2 uv) {
            this.vertex = vertex;
            this.normal = normal;
            this.uv = uv;
        }
        public override string ToString() {
            return $"vertex={vertex}, normal={normal},uv={uv}";
        }
    }

}