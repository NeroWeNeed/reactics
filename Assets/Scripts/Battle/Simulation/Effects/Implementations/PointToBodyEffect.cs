using System;
using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Entities;

namespace Reactics.Battle
{
    [Serializable]
    public struct PointToBodyEffect : IEffect<Point>
    {
        [SerializeNodeIndex(typeof(MapBodyTarget))]
        public int onBody;
        public void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, Point target, EntityCommandBuffer entityCommandBuffer)
        {
            if (onBody != -1)
            {
                var entity = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent(entity, new EffectIndexData(effectDataEntity, onBody));
                entityCommandBuffer.AddComponent(entity, new PointToMapBody
                {
                    point = target,
                    map = mapEntity
                });
            }
            entityCommandBuffer.DestroyEntity(cursorEntity);
        }
    }
}