using Reactics.Battle.Map;
using Unity.Entities;
using Unity.Jobs;

namespace Reactics.Battle {
    public interface IUtilityEffect : IEffect { }
    public interface IUtilityEffect<T> : IUtilityEffect, IEffect<T> where T : struct {
    }

    /// <summary>
    /// Utility Effect for chaining together actions.
    /// </summary>
    public struct LinearEffect : IUtilityEffect<MapBodyTarget>, IUtilityEffect<Point>, IUtilityEffect<MapBodyDirection> {
        public int effect;
        public int next;
        public JobHandle ScheduleJob(JobHandle handle, EffectAsset effectAsset, int effectIndex, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, MapBodyTarget target, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<MapBodyTarget>(handle, effectAsset, effectIndex, sourceEntity, source, mapEntity, map, target, entityCommandBuffer);
        public JobHandle ScheduleJob(JobHandle handle, EffectAsset effectAsset, int effectIndex, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, Point target, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<Point>(handle, effectAsset, effectIndex, sourceEntity, source, mapEntity, map, target, entityCommandBuffer);
        public JobHandle ScheduleJob(JobHandle handle, EffectAsset effectAsset, int effectIndex, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, MapBodyDirection target, EntityCommandBuffer entityCommandBuffer) => ScheduleJob<MapBodyDirection>(handle, effectAsset, effectIndex, sourceEntity, source, mapEntity, map, target, entityCommandBuffer);
        private JobHandle ScheduleJob<TTarget>(JobHandle handle, EffectAsset effectAsset, int effectIndex, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, TTarget target, EntityCommandBuffer entityCommandBuffer) where TTarget : struct {
            var targetEffect = (IEffect<TTarget>)effectAsset.effect[effect];
            var targetJobHandle = targetEffect.ScheduleJob(handle, effectAsset, effect, sourceEntity, source, mapEntity, map, target, entityCommandBuffer);
            if (next >= 0) {
                return ((IEffect<TTarget>)effectAsset.effect[next]).ScheduleJob(targetJobHandle, effectAsset, next, sourceEntity, source, mapEntity, map, target, entityCommandBuffer);
            }
            else {
                return targetJobHandle;
            }
        }
    }
}