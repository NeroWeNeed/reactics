using Unity.Collections;
using Unity.Entities;

namespace Reactics.Battle.Map
{
    [UpdateInGroup(typeof(MapSystemGroup))]
    [UpdateBefore(typeof(MapHighlightSystemGroup))]
    public class MapCollisionStateSystem : SystemBase
    {
        public EntityCommandBufferSystem commandBufferSystem;
        protected override void OnCreate()
        {
            commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            var buffer = commandBufferSystem.CreateCommandBuffer();
            Entities.WithAll<MapCollidableData, MapElement>().WithNone<MapCollidableSystemData>().ForEach((Entity entity) =>
             {
                 buffer.AddComponent<MapCollidableSystemData>(entity);
             }).Schedule();
            Entities.WithNone<MapCollidableData>().ForEach((Entity entity, in MapCollidableSystemData collidableSystemData, in MapElement mapElement) =>
            {
                var collisionState = GetComponent<MapCollisionState>(mapElement.value);
                if (collisionState.value.TryGetValue(collidableSystemData.point, out Entity collidableEntity) && collidableEntity.Equals(entity))
                {
                    collisionState.value.Remove(collidableSystemData.point);
                }
                buffer.RemoveComponent<MapCollidableSystemData>(entity);
            }).Schedule();
            Entities.WithChangeFilter<MapCollidableData>().ForEach((Entity entity, ref MapCollidableSystemData systemData, ref MapCollidableData data, in MapElement mapElement) =>
            {
                if (!systemData.point.Equals(data.point))
                {
                    var collisionState = GetComponent<MapCollisionState>(mapElement.value);
                    if (!collisionState.value.TryGetValue(data.point, out Entity collidableEntity) || !collidableEntity.Equals(entity))
                    {
                        if (collisionState.value.TryGetValue(systemData.point, out collidableEntity) && collidableEntity.Equals(entity))
                        {
                            collisionState.value.Remove(systemData.point);
                        }
                        collisionState.value.Add(data.point, entity);
                        systemData.point = data.point;
                    }
                    else
                    {
                        //TODO: Don't know how to handle this outside of snapback to original collidable point.
                        data.point = systemData.point;

                    }
                }
            }).Schedule();

            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}