using System;
using System.ComponentModel.DataAnnotations;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Core.Effects {
    /// <summary>
    /// Inflicts Damage onto the target
    /// </summary>
    [Serializable]
    public struct DamageEffect : IEffect<MapBodyTarget>, IEffectBehaviour<MapBodyTarget> {

        public byte damage;

        public void Invoke(EffectPayload<MapBodyTarget> payload, EntityCommandBuffer entityCommandBuffer) {
            var entity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(entity, new DamageTargetData(payload.target.entity, damage));
        }
        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<MapBodyTarget> payload, EntityCommandBuffer entityCommandBuffer) => this.ScheduleJobFrom(handle, payload, entityCommandBuffer);
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