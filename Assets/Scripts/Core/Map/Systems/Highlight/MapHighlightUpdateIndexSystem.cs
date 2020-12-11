using NeroWeNeed.Commons;
using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.Map {
    /// <summary>
    /// Pushes indices mapped in <see cref="MapHighlightTileSystem"/> to <see cref="MeshUpdateIndexData32"/> buffer to update meshes.
    /// </summary>
    [UpdateInGroup(typeof(MapHighlightSystemGroup))]
    [UpdateAfter(typeof(MapHighlightTileSystem))]
    public class MapHighlightUpdateIndexSystem : SystemBase {
        private EntityQuery query;
        protected override void OnUpdate() {
            var entityCount = query.CalculateEntityCount();
            if (entityCount > 0) {
                var indexBuffer = GetBufferFromEntity<MeshIndexUpdateData32>(false);
                Entities.WithChangeFilter<MapHighlightState>().ForEach((Entity entity, int entityInQueryIndex, ref MapHighlightState state, in DynamicBuffer<MapLayerRenderer> layerRenderer, in MapData mapData) =>
                {
                    if (state.dirty != 0) {
                        var processed = new NativeList<uint>(Allocator.Temp);

                        for (int i = 1; i < MapLayers.Count; i++) {

                            var bit = (ushort)(1 << (i - 1));
                            if ((state.dirty & bit) == 0)
                                continue;
                            var iter = state.states.GetValuesForKey(bit);
                            processed.Clear();
                            while (iter.MoveNext()) {
                                uint j = (uint)MapCommons.IndexOf(iter.Current, mapData.Width);
                                if (processed.Contains(j))
                                    continue;
                                for (byte k = 0; k < 6; k++)
                                    indexBuffer[layerRenderer[i].entity].Add(new MeshIndexUpdateData32 { Value = j * 4 + MapMeshCommons.TILE_INDICES[k] });
                                processed.Add(j);
                            }

                        }
                        state.dirty = 0;
                    }
                }).WithStoreEntityQueryInField(ref query).Schedule();
            }
        }

    }
}