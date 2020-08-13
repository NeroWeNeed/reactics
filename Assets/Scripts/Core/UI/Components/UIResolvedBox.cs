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
    public struct UIResolvedBox : IComponentData {
        public float4 value;
        public float2 Size => new float2(value.z - value.x, value.w - value.y);
        public float2 Position => new float2(value.x, value.y);
        public float2 Center => new float2(value.x + ((value.z - value.x) / 2f), value.y + ((value.w - value.y) / 2f));
        public float Width => value.z - value.x;
        public float Height => value.w - value.y;
    }

}