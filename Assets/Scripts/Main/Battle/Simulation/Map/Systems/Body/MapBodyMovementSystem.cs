using Unity.Entities;

namespace Reactics.Battle.Map
{
    [UpdateInGroup(typeof(MapBodyManagementSystemGroup))]
    [UpdateAfter(typeof(MapBodyPathFindingSystem))]
    public class MapBodyMovementSystem : SystemBase
    {
        private EntityCommandBufferSystem commandBufferSystem;
        private EntityQuery query;
        protected override void OnCreate()
        {
            commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            query = GetEntityQuery(typeof(MapBody), typeof(MapBodyPathFindingRoute), ComponentType.ReadOnly<MapElement>());
            RequireForUpdate(query);
        }
        protected override void OnUpdate()
        {
            var ecb = commandBufferSystem.CreateCommandBuffer();
            var time = Time.DeltaTime;
            Entities.WithAll<FindingPathInfo>().ForEach((Entity entity, ref MapBody body, ref DynamicBuffer<MapBodyPathFindingRoute> route, in MapElement mapElement) =>
            {
                var valid = true;
                if (route.Length > 0)
                {
                    float increment = time * route[0].speed;
                    while (increment > 0 && route.Length > 0)
                    {
                        var routeStep = route[0];
                        var old = routeStep.completion;
                        var next = old + increment;
                        increment = 0;
                        if (old < 0.5f && next >= 0.5f)
                        {
                            if (HasComponent<MapCollidableData>(entity))
                            {
                                if (!GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(routeStep.next, out Entity collidableEntity) || collidableEntity.Equals(entity))
                                {
                                    body.point = routeStep.next;
                                }
                                else
                                {
                                    valid = false;
                                    route.Clear();
                                    break;
                                }
                            }
                            else
                                body.point = routeStep.next;
                        }

                        if (next >= 1)
                        {
                            increment += next - 1;
                            route.RemoveAt(0);
                        }
                        else
                        {
                            routeStep.completion = next;
                            route[0] = routeStep;
                        }
                    };



                }
                if (route.Length <= 0)
                {
                    ecb.RemoveComponent<MapBodyPathFindingRoute>(entity);
                    if (valid)
                        ecb.RemoveComponent<FindingPathInfo>(entity);
                }
            }).Schedule();
            commandBufferSystem.AddJobHandleForProducer(Dependency);
        }
    }
}