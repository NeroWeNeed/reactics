using System;
using Reactics.Battle.Map;
using Unity.Entities;

namespace Reactics.Battle
{
    /// <summary>
    /// Inflicts Damage onto the target
    /// </summary>
    [Serializable]
    public struct DamageEffect : IEffect<MapBodyTarget>
    {

        public byte damage;
        public void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, MapBodyTarget target, EntityCommandBuffer entityCommandBuffer)
        {
            var entity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(entity, new DamageTargetData(target.entity, damage));
            entityCommandBuffer.DestroyEntity(cursorEntity);
        }
    }


    public struct DamageTargetData : IComponentData
    {
        public Entity target;

        public int damage;

        public DamageTargetData(Entity target, int damage)
        {
            this.target = target;
            this.damage = damage;
        }
    }
}