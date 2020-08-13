using Unity.Entities;

namespace Reactics.Core.Map {

    [UpdateInGroup(typeof(MapHighlightSystemGroup))]
    [UpdateBefore(typeof(MapHighlightTileSystem))]
    public class MapHighlightStateSystem : SystemBase {
        private EntityCommandBufferSystem commandBufferSystem;
        protected override void OnCreate() {
            commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate() {
            var buffer = commandBufferSystem.CreateCommandBuffer();
            var parallelBuffer = buffer.AsParallelWriter();
            Entities.WithNone<HighlightSystemTile>().ForEach((Entity entity, int entityInQueryIndex, in DynamicBuffer<HighlightTile> highlights) =>
            {
                buffer.AddBuffer<HighlightSystemTile>(entity);
            }).Schedule();
            Entities.WithNone<HighlightTile>().ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<HighlightSystemTile> systemHighlights, in MapElement mapElement) =>
            {
                for (int i = 0; i < systemHighlights.Length; i++) {
                    var highlight = systemHighlights[i];
                    var state = GetComponent<MapHighlightState>(mapElement.value);

                    for (int j = 1; j < MapLayers.Count; j++) {
                        var layer = MapLayers.Get(j);
                        if ((highlight.state & layer) != 0)
                            state.states.Remove(layer, highlight.point);
                    }
                    systemHighlights.RemoveAt(i--);
                    state.dirty |= highlight.state;
                    buffer.SetComponent(mapElement.value, state);



                }
                buffer.RemoveComponent<HighlightSystemTile>(entity);
            }).Schedule();
            commandBufferSystem.AddJobHandleForProducer(Dependency);

        }
    }
}