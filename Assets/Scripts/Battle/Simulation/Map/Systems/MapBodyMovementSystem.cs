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
            var translationData = GetComponentDataFromEntity<MapBodyTranslation>(true);

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
            var keys = BodyPoints.GetKeyArray(Allocator.Temp);
            bool clean = false;
            while (!clean)
            {
                clean = true;
                for (int i = 0; i < keys.Length; i++)
                {
                    if (BodyPoints[keys[i]].moving)
                        for (int j = 0; j < keys.Length; j++)
                        {
                            if (i == j)
                                continue;
                            if ((BodyPoints[keys[i]].next.Equals(BodyPoints[keys[j]].current) &&
                            BodyPoints[keys[j]].completion < 0.5f) ||
                            (BodyPoints[keys[i]].next.Equals(BodyPoints[keys[j]].next) && BodyPoints[keys[j]].completion > 0f))
                            {
                                Point t = BodyPoints[keys[i]].current;
                                BodyPoints.Remove(keys[i]);
                                BodyPoints.Add(keys[i], new MapBodyPoint(t));
                                clean = false;

                            }


                        }
                }
            }


            keys.Dispose();
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
            public void CancelMovement()
            {
                if (moving)
                {
                    moving = false;
                    completion = 0f;
                    priority = 0;
                    next = default;

                }
            }
        }
    }

    [UpdateInGroup(typeof(MapBodySystemGroup))]
    [UpdateAfter(typeof(MapBodyPathFindingSystem))]
    public class MapBodyMovementSystem : ComponentSystem
    {
        private BattleSimulationEntityCommandBufferSystem ecbSystem;
        private MapBodySystemGroup mbSystem;
        private EntityQuery query;

        protected override void OnCreate()
        {
            ecbSystem = World.GetOrCreateSystem<BattleSimulationEntityCommandBufferSystem>();
            mbSystem = World.GetExistingSystem<MapBodySystemGroup>();
            query = GetEntityQuery(typeof(MapBodyTranslationStep), typeof(MapBody), ComponentType.Exclude(typeof(MapBodyTranslation)));
            RequireForUpdate(query);

        }


        protected override void OnUpdate()
        {
            EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();

            float deltaTime = (float)BattleSimulationSystemGroup.SIMULATION_RATE;

            NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bodyPoints = mbSystem.BodyPoints;
            Entities.With(query).ForEach((Entity entity, DynamicBuffer<MapBodyTranslationStep> steps, ref MapBody body) =>
            {
                float amount = deltaTime * body.speed;
                int stepIndex = 0;
                MapBodyTranslationStep step;
                float newCompletion;
                while (amount > 0 && stepIndex < steps.Length)
                {
                    step = steps[stepIndex];
                    if (step.completion == 0f)
                    {
                        var keys = bodyPoints.GetKeyArray(Allocator.Temp);
                        for (int i = 0; i < keys.Length; i++)
                        {

                            if (keys[i] == entity.Index)
                                continue;
                            if ((bodyPoints[keys[i]].current.Equals(step.point) && bodyPoints[keys[i]].completion < 0.5f) || (bodyPoints[keys[i]].next.Equals(step.point) && bodyPoints[keys[i]].moving && (bodyPoints[keys[i]].completion > step.completion || (bodyPoints[keys[i]].completion == step.completion && bodyPoints[keys[i]].priority > body.speed) || (bodyPoints[keys[i]].completion == step.completion && bodyPoints[keys[i]].priority == body.speed && keys[i] > entity.Index))))
                            {
                                if (EntityManager.HasComponent<MapBodyTranslation>(entity))
                                    ecb.SetComponent(entity, new MapBodyTranslation
                                    {
                                        point = steps[steps.Length - 1].point
                                    });
                                else
                                    ecb.AddComponent(entity, new MapBodyTranslation
                                    {
                                        point = steps[steps.Length - 1].point
                                    });
                                ecb.RemoveComponent<MapBodyTranslationStep>(entity);
                                bodyPoints.Remove(entity.Index);
                                bodyPoints.Add(entity.Index, new MapBodySystemGroup.MapBodyPoint(body.point));
                                return;
                            }
                            if (body.point.Distance(step.point) > 1)
                            {
                                int2 temp = new int2(step.point.x - body.point.x, step.point.y - body.point.y);
                                Point c1 = body.point.ShiftX(temp.x);
                                Point c2 = body.point.ShiftY(temp.y);
                                if (keys.Any(ref bodyPoints, (ref int key, ref NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bp) => (bp[key].current.Equals(c1) && bp[key].completion <= 0.5f) || (bp[key].next.Equals(c1) && bp[key].completion > 0.5f)) &&
                                keys.Any(ref bodyPoints, (ref int key, ref NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bp) => (bp[key].current.Equals(c2) && bp[key].completion <= 0.5f) || (bp[key].next.Equals(c2) && bp[key].completion > 0.5f))
                                )
                                {
                                    if (EntityManager.HasComponent<MapBodyTranslation>(entity))
                                        ecb.SetComponent(entity, new MapBodyTranslation
                                        {
                                            point = steps[steps.Length - 1].point
                                        });
                                    else
                                        ecb.AddComponent(entity, new MapBodyTranslation
                                        {
                                            point = steps[steps.Length - 1].point
                                        });
                                    ecb.RemoveComponent<MapBodyTranslationStep>(entity);
                                    bodyPoints.Remove(entity.Index);
                                    bodyPoints.Add(entity.Index, new MapBodySystemGroup.MapBodyPoint(body.point));
                                    return;
                                }
                            }


                        }
                    }

                    newCompletion = step.completion + amount;


                    if (newCompletion >= 1.0f)
                    {
                        steps.RemoveAt(stepIndex);
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
                if (stepIndex < steps.Length)
                {
                    bodyPoints.Remove(entity.Index);
                    bodyPoints.Add(entity.Index, new MapBodySystemGroup.MapBodyPoint(body.point, steps[stepIndex].point, steps[stepIndex].completion, body.speed));
                }
                else
                {
                    ecb.RemoveComponent<MapBodyTranslationStep>(entity);
                    bodyPoints.Remove(entity.Index);
                    bodyPoints.Add(entity.Index, new MapBodySystemGroup.MapBodyPoint(body.point));
                }

            });

        }

    }


    /*     [UpdateInGroup(typeof(MapBodySystemGroup))]
        [UpdateAfter(typeof(MapBodyPathFindingSystem))]
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
                    bodyPoints = mbSystem.BodyPoints
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
                // :)
                [NativeDisableParallelForRestriction]
                public NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bodyPoints;

                //            public NativeHashMap<int, MapBodySystemGroup.MapBodyPoint>.ParallelWriter bodiesWriter;
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
                            var keys = bodyPoints.GetKeyArray(Allocator.Temp);
                            for (int i = 0; i < keys.Length; i++)
                            {
                                if (keys[i] == entity.Index)
                                    continue;
                                if ((bodyPoints[keys[i]].current.Equals(step.point) && bodyPoints[keys[i]].completion < 0.5f) || (bodyPoints[keys[i]].next.Equals(step.point) && bodyPoints[keys[i]].completion >= 0.5f))
                                {
                                    ecb.AddComponent(index, entity, new MapBodyTranslation
                                    {
                                        point = steps[0].point
                                    });
                                    ecb.RemoveComponent<MapBodyTranslationStep>(index, entity);
                                    return;
                                }
                                if (body.point.Distance(step.point) > 1)
                                {
                                    int2 temp = new int2(step.point.x - body.point.x, step.point.y - body.point.y);
                                    Point c1 = body.point.ShiftX(temp.x);
                                    Point c2 = body.point.ShiftY(temp.y);
                                    if (keys.Any(ref bodyPoints, (ref int key, ref NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bp) => bp[key].current.Equals(c1) || (bp[key].next.Equals(c1) && bp[key].moving)) &&
                                    keys.Any(ref bodyPoints, (ref int key, ref NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bp) => bp[key].current.Equals(c2) || (bp[key].next.Equals(c2) && bp[key].moving))
                                    )
                                    {
                                        ecb.AddComponent(index, entity, new MapBodyTranslation
                                        {
                                            point = steps[0].point
                                        });
                                        ecb.RemoveComponent<MapBodyTranslationStep>(index, entity);
                                        return;
                                    }
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
                        if (stepIndex >= 0)
                            bodyPoints.AsParallelWriter().TryAdd(entity.Index, new MapBodySystemGroup.MapBodyPoint(body.point, steps[stepIndex].point, newCompletion, body.speed));


                    }
                    if (stepIndex < 0)
                    {
                        ecb.RemoveComponent<MapBodyTranslationStep>(index, entity);
                        bodyPoints.AsParallelWriter().TryAdd(entity.Index, new MapBodySystemGroup.MapBodyPoint(body.point));
                    }



                }
            }


        } */


    [UpdateInGroup(typeof(MapBodySystemGroup))]
    public class MapBodyPathFindingSystem : JobComponentSystem
    {

        private BattleSimulationEntityCommandBufferSystem ecbSystem;
        private MapBodySystemGroup mbSystem;
        private EntityQuery query;

        private NativeHashMap<int, Entity> removes;

        private NativeMultiHashMap<Entity, MapBodyTranslationStep> steps;

        protected override void OnCreate()
        {
            RequireSingletonForUpdate<MapData>();
            ecbSystem = World.GetOrCreateSystem<BattleSimulationEntityCommandBufferSystem>();
            mbSystem = World.GetExistingSystem<MapBodySystemGroup>();
            query = GetEntityQuery(ComponentType.ReadOnly(typeof(MapBody)), typeof(MapBodyTranslation));
            RequireForUpdate(query);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            int entityCount = query.CalculateEntityCount();
            if (!removes.IsCreated)
            {
                removes = new NativeHashMap<int, Entity>(entityCount, Allocator.Persistent);
            }
            else if (entityCount > removes.Capacity)
            {
                removes.Dispose();
                removes = new NativeHashMap<int, Entity>(entityCount, Allocator.Persistent);
            }
            if (!steps.IsCreated)
            {
                steps = new NativeMultiHashMap<Entity, MapBodyTranslationStep>(64, Allocator.Persistent);
            }
            removes.Clear();
            steps.Clear();


            var pathFindingJob = new PathFindingJob
            {
                mapData = GetSingleton<MapData>(),
                bodyPoints = mbSystem.BodyPoints,
                stepData = GetBufferFromEntity<MapBodyTranslationStep>(true),
                removes = removes.AsParallelWriter(),
                steps = steps.AsParallelWriter()
            }.Schedule(this, inputDeps);
            var pathCleanupJob = new PathFindingCleanupJob
            {
                ecb = ecbSystem.CreateCommandBuffer(),
                removes = removes,
                steps = steps
            }.Schedule(pathFindingJob);
            ecbSystem.AddJobHandleForProducer(pathCleanupJob);

            return pathCleanupJob;
        }
        protected override void OnDestroy()
        {
            if (removes.IsCreated)
            {
                removes.Dispose();
            }
            if (steps.IsCreated)
            {
                steps.Dispose();
            }
        }
        protected override void OnStartRunning()
        {
            int entityCount = query.CalculateEntityCount();
            if (!removes.IsCreated)
            {
                removes = new NativeHashMap<int, Entity>(entityCount, Allocator.Persistent);
            }
            if (!steps.IsCreated)
            {
                steps = new NativeMultiHashMap<Entity, MapBodyTranslationStep>(64, Allocator.Persistent);
            }



        }
        //Not sure why this won't burstcompile
        public struct PathFindingJob : IJobForEachWithEntity<MapBody, MapBodyTranslation>
        {

            [ReadOnly]
            public MapData mapData;
            [ReadOnly]
            public NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bodyPoints;
            [ReadOnly]
            public BufferFromEntity<MapBodyTranslationStep> stepData;

            public NativeMultiHashMap<Entity, MapBodyTranslationStep>.ParallelWriter steps;
            public NativeHashMap<int, Entity>.ParallelWriter removes;



            public void Execute(Entity entity, int index, ref MapBody body, ref MapBodyTranslation translation)
            {
                if (body.point.Equals(translation.point) || mapData[translation.point].Inaccessible || stepData.Exists(entity))
                {

                    removes.TryAdd(index, entity);
                    //ecb.RemoveComponent<MapBodyTranslation>(index, entity);
                    return;
                }
                NativeList<PathNode> history = new NativeList<PathNode>(Allocator.Temp);
                NativeHeap<PathNode> frontier = new NativeHeap<PathNode>(Allocator.Temp);
                var origin = new PathNode(body.point.Distance(translation.point), 0, 0, body.point);
                frontier.Add(origin);
                history.Add(origin);
                PathNode current;
                PathNode closest = frontier.Peek();
                NativeArray<PathNode> expansion = new NativeArray<PathNode>(8, Allocator.Temp);
                while (frontier.Pop(out current) && !current.point.Equals(translation.point))
                {
                    closest = closest.distanceToDestination > current.distanceToDestination ? current : closest;
                    ExpandPoint(mapData, current.point, translation.point, body.point, ref expansion);
                    int historyIndex = history.IndexOf(current);
                    var keys = bodyPoints.GetKeyArray(Allocator.Temp);
                    for (int i = 0; i < expansion.Length; i++)
                    {
                        PathNode pathNode = expansion[i];
                        if (!pathNode.IsCreated || history.Contains(pathNode) || (translation.maxDistance >= 0 && pathNode.distanceFromOrigin >= translation.maxDistance))
                            goto SkipExpansionPoint;
                        for (int j = 0; j < bodyPoints.Length; j++)
                        {
                            if (keys[j] == entity.Index)
                                continue;
                            if (bodyPoints[keys[j]].current.Equals(pathNode.point) && !bodyPoints[keys[j]].moving)
                                goto SkipExpansionPoint;
                            //Make sure both tiles adjacent to diagonal aren't block
                            if (i % 2 == 1)
                                if (keys.Any(ref bodyPoints, ref expansion, (ref int key, ref NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bp, ref NativeArray<PathNode> e) => bp[key].current.Equals(e[MathUtils.FloorMod(i - 1, expansion.Length)])) &&
                                keys.Any(ref bodyPoints, ref expansion, (ref int key, ref NativeHashMap<int, MapBodySystemGroup.MapBodyPoint> bp, ref NativeArray<PathNode> e) => bp[key].current.Equals(e[(i + 1) % expansion.Length]))
                                )
                                    goto SkipExpansionPoint;

                        }


                        pathNode.previous = historyIndex;
                        history.Add(pathNode);
                        frontier.Add(pathNode);

                    SkipExpansionPoint:;
                    }
                    keys.Dispose();

                }
                var node = (current.IsCreated && current.point.Equals(translation.point) ? current : (closest.IsCreated && !closest.point.Equals(origin) ? closest : default));
                if (node.IsCreated && node.point.Equals(translation.point))
                {
                    while (node.previous != -1)
                    {

                        steps.Add(entity, new MapBodyTranslationStep
                        {
                            point = node.point
                        });

                        node = history[node.previous];
                    }
                }
                removes.TryAdd(index, entity);
                history.Dispose();
                frontier.Dispose();

                expansion.Dispose();


            }
        }
        [BurstCompile]
        public struct PathFindingCleanupJob : IJob
        {
            [ReadOnly]

            public NativeHashMap<int, Entity> removes;

            [ReadOnly]

            public NativeMultiHashMap<Entity, MapBodyTranslationStep> steps;

            public EntityCommandBuffer ecb;
            public void Execute()
            {
                var values = removes.GetValueArray(Allocator.Temp);
                for (int i = 0; i < values.Length; i++)
                {
                    ecb.RemoveComponent<MapBodyTranslation>(values[i]);
                }
                var keys = steps.GetKeyArray(Allocator.Temp);

                for (int i = 0; i < keys.Length; i++)
                {
                    var buffer = ecb.AddBuffer<MapBodyTranslationStep>(keys[i]);
                    var keyValues = steps.GetValuesForKey(keys[i]);
                    while (keyValues.MoveNext())
                    {
                        buffer.Add(keyValues.Current);
                    }

                }

            }
        }
        private static void ExpandPoint(MapData mapData, Point point, Point destination, Point origin, ref NativeArray<PathNode> expansion)
        {
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