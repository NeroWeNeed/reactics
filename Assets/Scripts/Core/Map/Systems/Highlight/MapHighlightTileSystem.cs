using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.Map {
    /// <summary>
    /// Map HighlightTile buffers to proper indices on proper submeshes.
    /// </summary>
    [UpdateInGroup(typeof(MapHighlightSystemGroup))]
    public class MapHighlightTileSystem : SystemBase {
        private EntityQuery query;


        protected override void OnUpdate() {
            var entityCount = query.CalculateEntityCount();
            if (entityCount > 0) {

                var mapData = GetComponentDataFromEntity<MapData>();
                var stateData = GetComponentDataFromEntity<MapHighlightState>(false);

                Entities.WithChangeFilter<HighlightTile>().ForEach((ref DynamicBuffer<HighlightSystemTile> systemHighlights, in MapElement mapElement, in DynamicBuffer<HighlightTile> highlights) =>
                {
                    var arr1 = highlights.AsNativeArray();
                    for (int i = 0; i < systemHighlights.Length; i++) {
                        var highlight = systemHighlights[i];
                        if (!arr1.Contains(highlight)) {
                            var state = stateData[mapElement.value];
                            for (int j = 1; j < MapLayers.Count; j++) {
                                var layer = MapLayers.Get(j);
                                if ((highlight.state & layer) != 0)
                                    state.states.Remove(layer, highlight.point);
                            }
                            systemHighlights.RemoveAt(i--);
                            state.dirty |= highlight.state;
                            stateData[mapElement.value] = state;
                        }
                    }
                    var arr2 = systemHighlights.AsNativeArray();
                    for (int i = 0; i < highlights.Length; i++) {

                        var highlight = highlights[i];
                        if (!arr2.Contains(highlight)) {

                            var state = stateData[mapElement.value];
                            for (int j = 1; j < MapLayers.Count; j++) {
                                var layer = MapLayers.Get(j);
                                if ((highlight.state & layer) != 0)
                                    state.states.Add(layer, highlight.point);
                            }
                            systemHighlights.Add((HighlightSystemTile)highlight);
                            arr2 = systemHighlights.AsNativeArray();
                            state.dirty |= highlight.state;
                            stateData[mapElement.value] = state;
                        }
                    }


                }).WithStoreEntityQueryInField(ref query).Schedule();

            }
        }
    }
}