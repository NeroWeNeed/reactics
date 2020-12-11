using System;
using System.Runtime.InteropServices;
using NeroWeNeed.BehaviourGraph;
using Reactics.Core.AssetDefinitions;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Core {
    [BurstCompile]
    public static class SampleEffects {


        [BurstCompile]
        [Behaviour("sample", typeof(SampleEffectData), typeof(EffectDelegate), "Sample Effect")]
        public static unsafe void Sample(IntPtr data, long dataLength, EffectDelegateInfo* source, EffectDelegateInfo* target, int targetLength, IntPtr entityCommandBuffer) {

        }
    }
    [Serializable]
    public unsafe struct SampleEffectData {

        public fixed int numbers[4];
        public Color32 color;
        public NodeIndex index;
    }
    [Serializable]
    public struct SampleEntry {
        public int id;
        public char character;
    }
    [VariableDefinition]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct SampleVariableDefinition {
        public int sample;

        public long sample2;
        public long sample3;
        public float sample4;
    }
}