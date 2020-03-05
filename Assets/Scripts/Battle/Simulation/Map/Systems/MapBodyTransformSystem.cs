using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Reactics.Battle
{
    /// <summary>
    /// System for adjusting map bodies according to their meshes.
    /// </summary>
    [UpdateInGroup(typeof(MapSystemGroup))]
    [UpdateBefore(typeof(MapBodyTransformSystem))]
    public class MapBodyMeshAdjuster : ComponentSystem
    {
        protected override void OnCreate()
        {
            RequireForUpdate(GetEntityQuery(MapArchetypes.RenderableBody.GetComponentTypes()));
        }
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref MapBody body, ref MapBodyMeshOffset meshOffset) =>
            {
                var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);
                body.offset = CalculateOffset((int)meshOffset.anchor, renderMesh.mesh.bounds.extents, meshOffset.offset);
            });
        }

        private float3 CalculateOffset(int anchor, float3 extents, float3 offset)
        {
            return new float3
            {
                x = ((anchor >> 4) - 1) * extents.x + offset.x,
                y = (((anchor >> 2) & 0b0011) - 1) * extents.y + offset.y,
                z = ((anchor & 0b000011) - 1) * extents.z + offset.z
            };
        }
    }



    /// <summary>
    /// System for positioning map bodies onto the map.
    /// </summary>
    [UpdateInGroup(typeof(MapSystemGroup))]
    public class MapBodyTransformSystem : JobComponentSystem
    {
        protected override void OnCreate()
        {
            RequireSingletonForUpdate<MapData>();
            RequireSingletonForUpdate<MapRenderData>();
            RequireForUpdate(GetEntityQuery(typeof(LocalToWorld), typeof(MapBody)));
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            var job = new TransformJob
            {
                renderData = GetSingleton<MapRenderData>(),
                mapData = GetSingleton<MapData>(),
                stepData = GetBufferFromEntity<MapBodyTranslationStep>(true)
            };
            return job.Schedule(this, inputDeps);
        }

        public struct TransformJob : IJobForEachWithEntity<LocalToWorld, MapBody>
        {
            [ReadOnly]
            public MapRenderData renderData;

            [ReadOnly]
            public BufferFromEntity<MapBodyTranslationStep> stepData;

            [ReadOnly]
            public MapData mapData;

            public void Execute(Entity entity, int index, ref LocalToWorld ltw, ref MapBody body)
            {
                float3 position = new float3(body.point.x + renderData.tileSize / 2 + body.offset.x, (mapData.map.Value[body.point].Elevation * renderData.elevationStep) + body.offset.y, body.point.y + renderData.tileSize / 2 + body.offset.z);

                if (stepData.Exists(entity))
                {
                    DynamicBuffer<MapBodyTranslationStep> steps = stepData[entity];
                    if (steps.Length > 0 && steps.IsCreated)
                    {
                        MapBodyTranslationStep step = steps[steps.Length - 1];
                        position += new float3((step.point.x - body.point.x) * step.completion, (mapData.map.Value[body.point].Elevation * renderData.elevationStep) - (mapData.map.Value[step.point].Elevation * renderData.elevationStep), (step.point.y - body.point.y) * step.completion);
                    }
                }
                ltw.Value = float4x4.Translate(position);
            }
        }
    }

}