using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using Reactics.Util;
using Unity.Transforms;
using Unity.Jobs;
using System;
using Unity.Mathematics;
using Unity.Burst;
using Reactics.Commons;
using System.Runtime.InteropServices;

namespace Reactics.Battle
{
    [UpdateInGroup(typeof(MapSystemGroup))]
    public class MapBodySystemGroup : ComponentSystemGroup
    {
        public NativeHashMap<int, MapBodyPoint> BodyPoints { get; private set; }

        protected override void OnUpdate()
        {
            BodyPoints.Clear();

            var stepData = GetBufferFromEntity<MapBodyTranslationStep>(true);

            Entities.ForEach((Entity entity, ref MapBody body) =>
            {
                if (stepData.Exists(entity))
                {
                    var step = stepData[entity];
                    if (step.IsCreated && step.Length > 0)
                        BodyPoints.Add(entity.Index, new MapBodyPoint(body.point, step[step.Length - 1].point, step[step.Length - 1].completion, body.speed));
                    else
                        BodyPoints.Add(entity.Index, new MapBodyPoint(body.point));
                }
                else
                    BodyPoints.Add(entity.Index, new MapBodyPoint(body.point));
            });
            base.OnUpdate();
        }
        protected override void OnStartRunning()
        {
            if (BodyPoints.IsCreated)
                BodyPoints.Dispose();
            BodyPoints = new NativeHashMap<int, MapBodyPoint>(8, Allocator.Persistent);
        }
        protected override void OnStopRunning()
        {
            if (BodyPoints.IsCreated)
                BodyPoints.Dispose();
        }
        public struct MapBodyPoint
        {
            public Point current;

            public Point next;

            public BlittableBool moving;

            public float completion;
            public float priority;
            public MapBodyPoint(Point current)
            {
                this.current = current;
                next = default;
                completion = 0f;
                priority = 0;
                moving = false;
            }
            public MapBodyPoint(Point current, Point next, float completion, float priority)
            {
                this.current = current;
                this.next = next;
                this.completion = completion;
                this.priority = priority;
                moving = true;
            }
        }
    }


    [UpdateInGroup(typeof(MapBodySystemGroup))]
    public class MapBodyMovementSystem : JobComponentSystem
    {
        private BattleSimulationEntityCommandBufferSystem ecbSystem;
        private MapBodySystemGroup mbSystem;

        protected override void OnCreate()
        {
            ecbSystem = World.GetOrCreateSystem<BattleSimulationEntityCommandBufferSystem>();
            mbSystem = World.GetExistingSystem<MapBodySystemGroup>();
            RequireForUpdate(GetEntityQuery(typeof(MapBodyTranslationStep), typeof(MapBody)));

        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {


            var isMoving = GetBufferFromEntity<MapBodyTranslationStep>(true);
            var job = new MovementJob
            {
                deltaTime = (float)BattleSimulationSystemGroup.SIMULATION_RATE,
                ecb = ecbSystem.CreateCommandBuffer().ToConcurrent(),
                bodies = mbSystem.BodyPoints
                //bodiesWriter = mbSystem.BodyPoints.AsParallelWriter()
            };

            var handle = job.Schedule(this, inputDeps);
            ecbSystem.AddJobHandleForProducer(handle);

            return handle;
        }


        public struct MovementJob : IJobForEachWithEntity_EBC<MapBodyTranslationStep, MapBody>
        {

            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly]
            public float deltaTime;
            [ReadOnly]
            public BufferFromEntity<MapBodyTranslationStep> stepData;
            [ReadOnly]
            public NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bodies;

            //public NativeHashMap<int, MapBodySystemGroup.MapBodyPoint>.ParallelWriter bodiesWriter;
            public void Execute(Entity entity, int index, DynamicBuffer<MapBodyTranslationStep> steps, ref MapBody body)
            {

                float amount = deltaTime * body.speed;
                int stepIndex = steps.Length - 1;
                MapBodyTranslationStep step;
                float newCompletion;
                while (amount > 0 && stepIndex >= 0)
                {
                    step = steps[stepIndex];
                    if (step.completion == 0f)
                    {
                        var keys = bodies.GetKeyArray(Allocator.Temp);
                        for (int i = 0; i < keys.Length; i++)
                        {
                            if (keys[i] == entity.Index)
                                continue;
                            if (bodies[keys[i]].current.Equals(step.point) && !bodies[keys[i]].moving)
                            {
                                ecb.AddComponent<MapBodyTranslation>(index, entity, new MapBodyTranslation
                                {
                                    point = steps[0].point
                                });
                                return;
                            }

                        }
                    }

                    newCompletion = step.completion + amount;


                    if (newCompletion >= 1.0f)
                    {
                        steps.RemoveAt(stepIndex);
                        stepIndex--;
                        amount -= newCompletion - step.completion;
                        body.point = step.point;
                    }
                    else
                    {
                        step.completion = newCompletion;
                        amount = 0;
                        steps[stepIndex] = step;
                    }
                }
                if (stepIndex < 0)
                {
                    ecb.RemoveComponent<MapBodyTranslationStep>(index, entity);
                }


            }
        }


    }


    [UpdateInGroup(typeof(MapBodySystemGroup))]
    public class MapBodyPathFindingSystem : JobComponentSystem
    {

        private BattleSimulationEntityCommandBufferSystem ecbSystem;
        private MapBodySystemGroup mbSystem;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<MapData>();
            ecbSystem = World.GetOrCreateSystem<BattleSimulationEntityCommandBufferSystem>();
            mbSystem = World.GetExistingSystem<MapBodySystemGroup>();
            RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly(typeof(MapBody)), typeof(MapBodyTranslation), ComponentType.Exclude(typeof(MapBodyTranslationStep))));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {


            var job = new PathFindingJob
            {
                ecb = ecbSystem.CreateCommandBuffer().ToConcurrent(),
                mapData = GetSingleton<MapData>(),
                bodyPoints = mbSystem.BodyPoints
            };
            var handle = job.Schedule(this, inputDeps);
            ecbSystem.AddJobHandleForProducer(handle);
            handle.Complete();
            return handle;
        }
        public struct PathFindingJob : IJobForEachWithEntity_ECC<MapBody, MapBodyTranslation>
        {


            public EntityCommandBuffer.Concurrent ecb;
            [ReadOnly]
            public MapData mapData;
            [ReadOnly]
            public NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bodyPoints;




            public void Execute(Entity entity, int index, ref MapBody body, ref MapBodyTranslation translation)
            {
                if (body.point.Equals(translation.point) || mapData[translation.point].Inaccessible)
                {
                    ecb.RemoveComponent<MapBodyTranslation>(index, entity);
                    return;
                }
                NativeList<PathNode> history = new NativeList<PathNode>(Allocator.Temp);
                NativeHeap<PathNode> frontier = new NativeHeap<PathNode>(Allocator.Temp);
                var origin = new PathNode(body.point.Distance(translation.point), 0, 0, body.point);
                frontier.Add(origin);
                history.Add(origin);
                PathNode current;
                while (frontier.Pop(out current) && !current.point.Equals(translation.point))
                {
                    NativeArray<PathNode> expansion = ExpandPoint(mapData, current.point, translation.point, body.point);

                    int historyIndex = history.IndexOf(current);
                    var keys = bodyPoints.GetKeyArray(Allocator.Temp);
                    for (int i = 0; i < expansion.Length; i++)
                    {
                        PathNode pathNode = expansion[i];
                        for (int j = 0; j < bodyPoints.Length; j++)
                        {
                            if (keys[j] == entity.Index)
                                continue;
                            if (bodyPoints[keys[j]].current.Equals(pathNode.point) && !bodyPoints[keys[j]].moving)
                                goto SkipExpansionPoint;
                        }

                        if (pathNode.IsCreated && !history.Contains(pathNode))
                        {
                            pathNode.previous = historyIndex;
                            history.Add(pathNode);
                            frontier.Add(pathNode);
                        }
                    SkipExpansionPoint:;
                    }
                    keys.Dispose();
                    expansion.Dispose();
                }
                if (current.IsCreated && current.point.Equals(translation.point))
                {
                    DynamicBuffer<MapBodyTranslationStep> steps = ecb.AddBuffer<MapBodyTranslationStep>(index, entity);
                    while (current.previous != -1)
                    {
                        steps.Add(new MapBodyTranslationStep
                        {
                            point = current.point
                        });

                        current = history[current.previous];
                    }
                }
                ecb.RemoveComponent<MapBodyTranslation>(index, entity);
                history.Dispose();
                frontier.Dispose();


            }
        }
        private static NativeArray<PathNode> ExpandPoint(MapData mapData, Point point, Point destination, Point origin)
        {
            NativeArray<PathNode> expansion = new NativeArray<PathNode>(8, Allocator.Temp);
            BlittableTile tile;
            Point cornerPoint, sidePoint;

            for (int i = 0; i < 8; i += 2)
            {

                if (point.Shift((int)math.round(math.cos(i * math.PI / 4f)), (int)math.round(math.sin(i * math.PI / 4f)), mapData.Width, mapData.Length, out Point tilePoint))
                {

                    tile = mapData[tilePoint];
                    if (tile.Inaccessible)
                        continue;
                    expansion[i] = new PathNode(tilePoint.Distance(destination), origin.Distance(tilePoint), point.Distance(tilePoint), tilePoint);
                    if (point.Shift((int)math.round(math.cos((i + 1) % 8 * math.PI / 4f)), (int)math.round(math.sin((i + 1) % 8 * math.PI / 4f)), mapData.Width, mapData.Length, out cornerPoint)
                    && point.Shift((int)math.round(math.cos((i + 2) % 8 * math.PI / 4f)), (int)math.round(math.sin((i + 2) % 8 * math.PI / 4f)), mapData.Width, mapData.Length, out sidePoint)
                    && !mapData[sidePoint].Inaccessible
                    && !mapData[cornerPoint].Inaccessible
                    )
                    {
                        expansion[(i + 1) % 8] = new PathNode(cornerPoint.Distance(destination), origin.Distance(cornerPoint), point.Distance(cornerPoint), cornerPoint);
                    }
                    if (point.Shift((int)math.round(math.cos(MathUtils.FloorMod(i - 1, 8) * math.PI / 4f)), (int)math.round(math.sin(MathUtils.FloorMod(i - 1, 8) * math.PI / 4f)), mapData.Width, mapData.Length, out cornerPoint)
                    && point.Shift((int)math.round(math.cos(MathUtils.FloorMod(i - 2, 8) * math.PI / 4f)), (int)math.round(math.sin(MathUtils.FloorMod(i - 2, 8) * math.PI / 4f)), mapData.Width, mapData.Length, out sidePoint)
                    && !mapData[sidePoint].Inaccessible
                    && !mapData[cornerPoint].Inaccessible
)
                    {
                        expansion[MathUtils.FloorMod(i - 1, 8)] = new PathNode(cornerPoint.Distance(destination), origin.Distance(cornerPoint), point.Distance(cornerPoint), cornerPoint);
                    }
                }

            }
            return expansion;
        }


        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct PathNode : IComparable<PathNode>, IEquatable<PathNode>
        {
            public uint distanceToDestination;

            public uint distanceFromOrigin;
            public uint stride;

            public Point point;

            public int previous;
            public BlittableBool IsCreated { get; }

            public PathNode(uint distanceToDestination, uint distanceFromOrigin, uint stride, Point point, int previous = -1)
            {
                this.distanceToDestination = distanceToDestination;
                this.distanceFromOrigin = distanceFromOrigin;
                this.stride = stride;
                this.point = point;
                this.previous = previous;
                IsCreated = true;
            }

            public int CompareTo(PathNode other)
            {
                var distanceComparision = (distanceToDestination).CompareTo(other.distanceToDestination);
                var strideComparison = other.stride.CompareTo(stride);
                return distanceComparision != 0 ? distanceComparision : strideComparison;
            }

            public bool Equals(PathNode other)
            {
                return distanceToDestination.Equals(other.distanceToDestination) && stride.Equals(other.stride) && point.Equals(other.point);
            }
            public override string ToString()
            {
                return $"{point}, {distanceToDestination}, {stride}";
            }
        }
    }

}