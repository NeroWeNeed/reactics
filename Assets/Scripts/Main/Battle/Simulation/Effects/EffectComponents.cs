using System;
using System.Collections.Generic;
using Reactics.Battle;
using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.AddressableAssets;

[assembly: RegisterGenericComponentType(typeof(EffectTarget<Point>))]
[assembly: RegisterGenericComponentType(typeof(EffectTarget<MapBodyTarget>))]
[assembly: RegisterGenericComponentType(typeof(EffectTarget<MapBodyDirection>))]

namespace Reactics.Battle {

    public struct Effect : IComponentData {
        public Resource value;
    }
    public struct EffectTarget<TTarget> : IComponentData where TTarget : struct {
        public TTarget value;
    }
    public struct EffectIndex : IComponentData {
        public int value;
    }
    public struct EffectSource : IComponentData {
        public Entity value;

    }
}