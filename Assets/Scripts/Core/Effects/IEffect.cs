using System;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Core.Effects {
    public struct BaseEffect : IEffect {

    }
    public interface IEffect {


    }
    [ConcreteTypeColor("#00F0D3", typeof(Point))]
    [ConcreteTypeColor("#E3C242", typeof(MapBodyDirection))]
    [ConcreteTypeColor("#E32BD8", typeof(MapBodyTarget))]
    public interface IEffect<TTarget> : IEffect where TTarget : struct {
        JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<TTarget> payload, EntityCommandBuffer entityCommandBuffer);
    }

    public struct EffectPayload<TTarget> where TTarget : struct {
        public Entity sourceEntity;
        public MapBody source;
        public Entity mapEntity;
        public MapData map;
        public TTarget target;
    }
    public interface IEffectBehaviour<TTarget> where TTarget : struct {
        void Invoke(EffectPayload<TTarget> payload, EntityCommandBuffer entityCommandBuffer);
    }
    public static class IEffectUtility {
        public static JobHandle ScheduleJobFrom<TEffect, TTarget>(this TEffect effect, JobHandle handle, EffectPayload<TTarget> payload, EntityCommandBuffer entityCommandBuffer) where TEffect : struct, IEffect<TTarget>, IEffectBehaviour<TTarget> where TTarget : struct {
            UnsafeUtility.As
            return new JobEffect<TEffect, TTarget>
            {
                effect = effect,
                payload = payload,
                entityCommandBuffer = entityCommandBuffer
            }.Schedule(handle);
        }
    }
    [BurstCompile]
    public struct JobEffect<TEffect, TTarget> : IJob where TEffect : struct, IEffect<TTarget>, IEffectBehaviour<TTarget> where TTarget : struct {

        public TEffect effect;

        public EffectPayload<TTarget> payload;

        public EntityCommandBuffer entityCommandBuffer;

        public void Execute() {
            effect.Invoke(payload, entityCommandBuffer);
        }
    }
}