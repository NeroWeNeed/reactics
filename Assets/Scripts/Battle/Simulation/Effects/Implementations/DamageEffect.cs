using System;
using System.ComponentModel.DataAnnotations;
using Reactics.Battle.Map;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Battle {
    /// <summary>
    /// Inflicts Damage onto the target
    /// </summary>
    [Serializable]
    public struct DamageEffect : IEffect<MapBodyTarget>, IEffectBehaviour<MapBodyTarget> {
        [Range(10, 20)]
        public byte damage;
        public void Invoke(Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, MapBodyTarget target, EntityCommandBuffer entityCommandBuffer) {
            var entity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(entity, new DamageTargetData(target.entity, damage));
        }
        public JobHandle ScheduleJob(JobHandle handle, EffectAsset effectAsset, int effectIndex, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, MapBodyTarget target, EntityCommandBuffer entityCommandBuffer) => IEffectUtility.ScheduleJobFrom(handle, this, sourceEntity, source, mapEntity, map, target, entityCommandBuffer);
    }

    public struct DamageTargetData : IComponentData {
        public Entity target;

        public int damage;

        public DamageTargetData(Entity target, int damage) {
            this.target = target;
            this.damage = damage;
        }
    }
}