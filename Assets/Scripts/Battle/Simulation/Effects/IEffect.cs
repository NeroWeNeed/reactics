using System;
using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Entities;
using Unity.Jobs;

namespace Reactics.Battle {
    public interface IEffect {


    }
    public interface IEffect<TTarget> : IEffect where TTarget : struct {
        JobHandle ScheduleJob(JobHandle handle, EffectAsset effectAsset, int effectIndex, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, TTarget target, EntityCommandBuffer entityCommandBuffer);
    }

    public interface IEffectBehaviour<TTarget> where TTarget : struct {
        void Invoke(Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, TTarget target, EntityCommandBuffer entityCommandBuffer);
    }
    public static class IEffectUtility {
        public static JobHandle ScheduleJobFrom<TEffect, TTarget>(JobHandle handle, TEffect effect, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, TTarget target, EntityCommandBuffer entityCommandBuffer) where TEffect : struct, IEffect<TTarget>, IEffectBehaviour<TTarget> where TTarget : struct {
            return new IJobEffect<TEffect, TTarget>
            {
                effect = effect,
                sourceEntity = sourceEntity,
                source = source,
                mapEntity = mapEntity,
                map = map,
                target = target,
                entityCommandBuffer = entityCommandBuffer
            }.Schedule(handle);
        }
    }

    public struct IJobEffect<TEffect, TTarget> : IJob where TEffect : struct, IEffect<TTarget>, IEffectBehaviour<TTarget> where TTarget : struct {

        public TEffect effect;
        public Entity sourceEntity;

        public MapBody source;

        public Entity mapEntity;
        public MapData map;

        public TTarget target;

        public EntityCommandBuffer entityCommandBuffer;

        public void Execute() {
            effect.Invoke(sourceEntity, source, mapEntity, map, target, entityCommandBuffer);
        }
    }
}