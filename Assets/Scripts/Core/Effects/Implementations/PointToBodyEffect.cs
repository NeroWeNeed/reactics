using System;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Core.Effects {
    [Serializable]
    public struct PointToBodyEffect : IEffect<Point> {
        [SerializeNodeIndex(typeof(IEffect<MapBodyTarget>))]
        [SerializeField]
        public IndexReference onBody;

        public JobHandle ScheduleJob(JobHandle handle, EntityManager entityManager, EffectAsset effectAsset, Resource effectResource, int effectIndex, EffectPayload<Point> payload, EntityCommandBuffer entityCommandBuffer) {
            var query = entityManager.CreateEntityQuery(typeof(MapBody), typeof(MapElement));
            var chunks = query.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out JobHandle archetypeHandle);
            var job = new PointToBodyEffectJob
            {
                mapBodyType = entityManager.GetComponentTypeHandle<MapBody>(true),
                mapElementType = entityManager.GetComponentTypeHandle<MapElement>(true),
                resource = effectResource,
                onBodyIndex = onBody.index,
                payload = payload,
                entityCommandBuffer = entityCommandBuffer
            }.Schedule(query, handle);

            query.Dispose();
            return job;
        }

        public struct PointToBodyEffectJob : IJobChunk {
            public ComponentTypeHandle<MapBody> mapBodyType;
            public ComponentTypeHandle<MapElement> mapElementType;
            public EntityTypeHandle entityType;
            public Resource resource;
            public EffectPayload<Point> payload;
            public EntityCommandBuffer entityCommandBuffer;
            public int onBodyIndex;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
                var bodies = chunk.GetNativeArray(mapBodyType);
                var elements = chunk.GetNativeArray(mapElementType);
                var entities = chunk.GetNativeArray(entityType);

                for (int i = 0; i < entities.Length; i++) {
                    if (elements[i].value == payload.mapEntity && bodies[i].point == payload.target) {
                        var entity = entityCommandBuffer.CreateEntity();

                    }
                }
            }
        }
    }
}