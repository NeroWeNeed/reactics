using Reactics.Battle.Map;
using Unity.Entities;

namespace Reactics.Battle
{
    public class EffectDataPointToBodySystem : SystemBase
    {
        private EntityQuery mapBodyQuery;

        private EntityQuery conversionQuery;

        private EntityCommandBufferSystem entityCommandBufferSystem;
        protected override void OnCreate()
        {

            mapBodyQuery = GetEntityQuery(typeof(MapBody), typeof(MapBody), typeof(MapElement));
            //conversionQuery = GetEntityQuery(typeof(PointToMapBody), typeof())
            entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();
            var body = GetComponentDataFromEntity<MapBody>(true);
            Entities.ForEach((Entity entity, in PointToMapBodyResult pointToMapBodyResult, in EffectIndexData effectIndexData) =>
            {
                if (pointToMapBodyResult.Found)
                {
                    var cursor = ecb.CreateEntity();
                    ecb.AddComponent(cursor, new DoEffect<MapBody>(effectIndexData.effectDataEntity, effectIndexData.index, pointToMapBodyResult.mapBodyEntity, body[pointToMapBodyResult.mapEntity]));
                }
                
                ecb.DestroyEntity(entity);
            }).Schedule();

        }
    }
}