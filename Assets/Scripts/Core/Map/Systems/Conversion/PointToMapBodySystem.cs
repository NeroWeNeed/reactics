using Unity.Collections;
using Unity.Entities;

namespace Reactics.Core.Map {

    public class PointToMapBodyConversionSystem : SystemBase {
        private EntityCommandBufferSystem entityCommandBufferSystem;

        private EntityQuery conversionQuery;
        private EntityQuery mapBodyQuery;
        protected override void OnCreate() {
            mapBodyQuery = GetEntityQuery(typeof(MapBody), typeof(MapElement));
            conversionQuery = GetEntityQuery(typeof(PointToMapBody));
            entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            RequireForUpdate(conversionQuery);
        }
        protected override void OnUpdate() {
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();
            var entities = conversionQuery.ToEntityArray(Allocator.TempJob);
            var bodyData = GetComponentDataFromEntity<MapBody>(true);
            var elementData = GetComponentDataFromEntity<MapElement>(true);
            Entities.ForEach((Entity entity, in PointToMapBody p2mb) =>
            {
                int match = -1;
                for (int i = 0; i < entities.Length; i++) {
                    if (elementData[entities[i]].value.Equals(p2mb.map) && bodyData[entities[i]].point.Equals(p2mb.point)) {
                        match = i;
                        break;
                    }
                }
                ecb.AddComponent(entity, new PointToMapBodyResult
                {
                    point = p2mb.point,
                    mapBodyEntity = match == -1 ? Entity.Null : entities[match],
                    mapEntity = p2mb.map
                });
                ecb.RemoveComponent<PointToMapBody>(entity);
            }).Schedule();
        }
    }
}