using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Effects;
using Reactics.Core.Map;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.AddressableAssets;


[assembly: RegisterGenericComponentType(typeof(EffectTarget<PointTarget>))]
[assembly: RegisterGenericComponentType(typeof(EffectTarget<MapBodyTarget>))]
[assembly: RegisterGenericComponentType(typeof(EffectTarget<DirectionTarget>))]
namespace Reactics.Core.Effects {

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
    public struct MapTileEffect : IBufferElementData {
        public Point point;
        public Reactics.Core.Effects.OldEffect effect;
    }

}