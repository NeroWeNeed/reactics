using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Core.Effects {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class EffectProcessingSystemGroup : ComponentSystemGroup { }
    public abstract class BaseEffectProcessingSystem<TTargetType> : SystemBase where TTargetType : struct {
        public abstract TargetType TargetType { get; }
        private EntityCommandBufferSystem entityCommandBufferSystem;
        public ResourceManager effectResourceSystem;
        protected EntityQuery query;
        protected override void OnCreate() {
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            effectResourceSystem = World.GetResourceSystem();
            query = GetEntityQuery(ComponentType.ReadOnly<Effect>(), ComponentType.ReadOnly<EffectSource>(), ComponentType.ReadOnly<MapElement>(), ComponentType.ReadOnly<EffectTarget<TTargetType>>());
            RequireForUpdate(query);
        }
        protected override void OnUpdate() {
            var indexData = GetComponentDataFromEntity<EffectIndex>(true);
            var mapBodyData = GetComponentDataFromEntity<MapBody>(true);
            var mapData = GetComponentDataFromEntity<MapData>(true);
            var ecb = entityCommandBufferSystem.CreateCommandBuffer();
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob);
            var referenceElements = GetComponentTypeHandle<Effect>(true);
            var targetElements = GetComponentTypeHandle<EffectTarget<TTargetType>>(true);
            var sourceElements = GetComponentTypeHandle<EffectSource>(true);
            var mapElementElements = GetComponentTypeHandle<MapElement>(true);
            var entityElements = GetEntityTypeHandle();
            for (int c = 0; c < chunks.Length; c++) {
                var chunk = chunks[c];
                var referenceElementData = chunk.GetNativeArray(referenceElements);
                var targetElementData = chunk.GetNativeArray(targetElements);
                var sourceElementData = chunk.GetNativeArray(sourceElements);
                var mapElementElementData = chunk.GetNativeArray(mapElementElements);
                var entityElementData = chunk.GetNativeArray(entityElements);
                for (int i = 0; i < entityElementData.Length; i++) {
                    if (!ProduceJob(indexData, mapBodyData, mapData, ecb, entityElementData[i], referenceElementData[i], targetElementData[i], sourceElementData[i], mapElementElementData[i], out JobHandle jobHandle)) {
                        jobHandle = this.Dependency;
                    }
                    var cleanupHandle = new DestroyEntityJob
                    {
                        entityCommandBuffer = ecb,
                        entity = entityElementData[i]
                    }.Schedule(jobHandle);
                    entityCommandBufferSystem.AddJobHandleForProducer(cleanupHandle);
                }
            }
            chunks.Dispose();
        }

        protected bool ProduceJob(
            ComponentDataFromEntity<EffectIndex> indexData,
            ComponentDataFromEntity<MapBody> mapBodyData,
            ComponentDataFromEntity<MapData> mapData,
            EntityCommandBuffer ecb,
            Entity entity,
            Effect reference,
            EffectTarget<TTargetType> target,
            EffectSource source,
            MapElement mapElement,
            out JobHandle jobHandle) {
            EffectAsset effectAsset = effectResourceSystem[reference.value] as EffectAsset;

            /*             if (effectAsset?.Type == TargetType) {

                            var payload = new EffectPayload<TTargetType>
                            {
                                sourceEntity = source.value,
                                source = mapBodyData[source.value],
                                mapEntity = mapElement.value,
                                map = mapData[mapElement.value],
                                target = target.value
                            };
                            if (indexData.HasComponent(entity)) {
                                var index = indexData[entity].value;
                                if (index >= 0 && index < effectAsset.EffectCount) {
                                    var t = (IEffect<TTargetType>)effectAsset.components[index];
                                    var job = t.ScheduleJob(this.Dependency, EntityManager, effectAsset, reference.value, index, payload, ecb);
                                    if (!job.Equals(this.Dependency)) {
                                        jobHandle = job;
                                        return true;
                                    }
                                }
                            }
                            else {
                                NativeList<JobHandle> jobs = new NativeList<JobHandle>(effectAsset.RootCount, Allocator.Temp);
                                for (int r = 0; r < effectAsset.RootCount; r++) {
                                    var rootIndex = effectAsset.roots[r];
                                    var effect = (IEffect<TTargetType>)effectAsset.components[rootIndex];
                                    var job = effect.ScheduleJob(this.Dependency, EntityManager, effectAsset, reference.value, rootIndex, payload, ecb);
                                    if (!job.Equals(this.Dependency))
                                        jobs.Add(job);
                                }
                                if (jobs.Length > 0) {
                                    jobHandle = JobHandle.CombineDependencies(jobs);
                                    return true;
                                }
                            }
                        } */
            jobHandle = default;
            return false;
        }

        public struct DestroyEntityJob : IJob {
            public EntityCommandBuffer entityCommandBuffer;
            public Entity entity;
            public void Execute() {
                entityCommandBuffer.DestroyEntity(entity);
            }
        }
    }
    [UpdateInGroup(typeof(EffectProcessingSystemGroup))]
    public class PointEffectProcessingSystem : BaseEffectProcessingSystem<Point> {
        public override TargetType TargetType => TargetType.Point;
    }
    [UpdateInGroup(typeof(EffectProcessingSystemGroup))]
    public class MapBodyDirectionEffectProcessingSystem : BaseEffectProcessingSystem<MapBodyDirection> {
        public override TargetType TargetType => TargetType.Direction;
    }
    [UpdateInGroup(typeof(EffectProcessingSystemGroup))]
    public class MapBodyTargetEffectProcessingSystem : BaseEffectProcessingSystem<MapBodyTarget> {
        public override TargetType TargetType => TargetType.Body;
    }
}