using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Reactics.Core.Map {
    [UpdateInGroup(typeof(MapBodyManagementSystemGroup))]
    [UpdateAfter(typeof(MapBodyMovementSystem))]
    public class MapBodyToWorldSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<RenderMesh>().WithChangeFilter<MapBody>().WithNone<MapBodyPathFindingRoute>().ForEach((Entity entity, ref LocalToWorld ltw, in MapBody body, in MapElement mapElement, in RenderBounds bounds) =>
            {

                var info = GetComponent<MapRenderInfo>(mapElement.value);
                var map = GetComponent<MapData>(mapElement.value);

                float3 location = new float3(body.point.x * info.tileSize + info.tileSize / 2f,
                map.Elevation * info.elevationStep + map.GetTile(body.point).Elevation * info.elevationStep,
                body.point.y * info.tileSize + info.tileSize / 2f);
                location += body.anchor.XYZ(bounds.Value.Extents);
                if (HasComponent<Translation>(entity))
                    location += GetComponent<Translation>(entity).Value;
                quaternion rotation;
                if (body.direction > 0) {
                    rotation = quaternion.AxisAngle(new float3(0, 1, 0), ((byte)body.direction - 1) * math.radians(45));
                }
                else
                    rotation = quaternion.identity;
                float3 scale = new float3(1, 1, 1);
                ltw.Value = float4x4.TRS(location, rotation, new float3(1));
            }).ScheduleParallel();
        }

    }
}