using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Reactics.Battle.Map
{
    [UpdateInGroup(typeof(MapBodyManagementSystemGroup))]
    [UpdateAfter(typeof(MapBodyToWorldSystem))]
    public class MapBodyRouteToWorldSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<RenderMesh>().WithChangeFilter<MapBody>().ForEach((Entity entity, ref LocalToWorld ltw, in MapBody body, in DynamicBuffer<MapBodyPathFindingRoute> route, in MapElement mapElement, in RenderBounds bounds) =>
            {
                if (route.Length > 0)
                {
                    var step = route[0];
                    var info = GetComponent<MapRenderInfo>(mapElement.value);
                    var map = GetComponent<MapData>(mapElement.value);
                    float3 location = new float3(step.previous.x * info.tileSize + info.tileSize / 2f,
                    map.Elevation * info.elevationStep + map.GetTile(step.previous).Elevation * info.elevationStep,
                    step.previous.y * info.tileSize + info.tileSize / 2f);
                    location += body.anchor.XYZ(bounds.Value.Extents);
                    location += new float3((step.next.x - step.previous.x) * info.tileSize, (map.GetTile(step.next).Elevation - map.GetTile(step.previous).Elevation) * info.elevationStep, (step.next.y - step.previous.y) * info.tileSize) * step.completion;
                    if (HasComponent<Translation>(entity))
                        location += GetComponent<Translation>(entity).Value;
                    quaternion rotation;
                    if (body.direction > 0)
                    {
                        rotation = quaternion.AxisAngle(new float3(0, 1, 0), ((byte)body.direction - 1) * math.radians(45));
                    }
                    else
                        rotation = quaternion.identity;
                    float3 scale = new float3(1, 1, 1);
                    ltw.Value = float4x4.TRS(location, rotation, new float3(1));
                }
            }).Schedule();
        }
    }
}