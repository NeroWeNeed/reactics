using System;
using NeroWeNeed.BehaviourGraph;
using Reactics.Core.AssetDefinitions;
using Unity.Burst;

namespace Reactics.Core.Effects {
    [BurstCompile]
    public unsafe static class SkillEffectTable {

        [BurstCompile]
        [Behaviour("damage", typeof(DamageEffect), typeof(EffectDelegate), "Damage Effect")]
        public static void DoDamage(IntPtr data, long dataLength, EffectDelegateInfo* source, EffectDelegateInfo* target, int targetLength, IntPtr entityCommandBuffer) {

        }
        [BurstCompile]
        [Behaviour("track", typeof(TrackingEffect), typeof(EffectDelegate), "Tracking Effect")]
        public static void Track(IntPtr data, long dataLength, EffectDelegateInfo* source, EffectDelegateInfo* target, int targetLength, IntPtr entityCommandBuffer) {

        }
    }

}