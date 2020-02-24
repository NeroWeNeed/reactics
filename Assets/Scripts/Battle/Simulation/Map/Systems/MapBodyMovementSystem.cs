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

namespace Reactics.Battle
{
    /* 
            [UpdateInGroup(typeof(MapSystemGroup))]
            [UpdateBefore(typeof(MapBodyPathFindingSystem))]
            public class MapBodyMovementSystem : JobComponentSystem
            {
                EntityQuery query;

                private EntityCommandBufferSystem ecbSystem;
                protected override void OnCreate()
                {

                    query = EntityManager.CreateEntityQuery(typeof(MapBodyTranslationPoint), typeof(MapBody));
                    ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
                    RequireForUpdate(query);
                    RequireSingletonForUpdate<MapData>();
                }
                protected override JobHandle OnUpdate(JobHandle inputDeps)
                {
                    MoveJob job = new MoveJob
                    {
                        deltaTime = World.Time.DeltaTime,
                        CommandBuffer = ecbSystem.CreateCommandBuffer().ToConcurrent()
                    };
                    var handle = job.Schedule(query, inputDeps);
                    ecbSystem.AddJobHandleForProducer(handle);
                    return handle;
                }
                private struct MoveJob : IJobForEachWithEntity_EBC<MapBodyTranslationPoint, MapBody>
                {
                    [ReadOnly]
                    public float deltaTime;

                    public EntityCommandBuffer.Concurrent CommandBuffer;
                    public void Execute(Entity entity, int index, DynamicBuffer<MapBodyTranslationPoint> tiles, ref MapBody body)
                    {
                        float distance = body.speed * deltaTime;
                        MapBodyTranslationPoint mapBodyTranslationPoint;
                        while (distance > 0 && tiles.Length > 0)
                        {
                            mapBodyTranslationPoint = tiles[0];
                            float newDistance = mapBodyTranslationPoint.completion + distance;


                            if (newDistance >= 1)
                            {
                                distance -= 1 - mapBodyTranslationPoint.completion;
                                body.point = tiles[0].point;
                                tiles.RemoveAt(0);
                            }
                            else
                            {
                                mapBodyTranslationPoint.completion = newDistance;
                                tiles[0] = mapBodyTranslationPoint;
                                distance = 0;
                            }
                        }
                    }
                }
            }


        //TODO: No Min Heap has been implemented. Currently sorts frontier every iteration. Implement min heap to improve performance
        //TODO: Body Collision Detection
        //TODO: Body Moving Collision Detection
        /// <summary>
        /// Used for determining paths from MapBodyTranslation Components.
        /// </summary>
        [UpdateInGroup(typeof(MapSystemGroup))]

        public class MapBodyPathFindingSystem : JobComponentSystem
        {
            private EntityQuery query;
            private EntityCommandBufferSystem ecbSystem;
            protected override void OnCreate()
            {
                query = EntityManager.CreateEntityQuery(typeof(MapBodyTranslation), typeof(MapBody));
                RequireForUpdate(query);
                RequireSingletonForUpdate<MapData>();
            }
            protected override void OnStartRunning()
            {
                ecbSystem = World.GetExistingSystem<BattleSimulationEntityCommandBufferSystem>();
                Debug.Log(ecbSystem);
            }
            protected override JobHandle OnUpdate(JobHandle inputDeps)
            {

                var job = new FindPathJob
                {
                    bodies = GetEntityQuery(typeof(MapBody)).ToEntityArray(Allocator.TempJob),
                    mapData = GetSingleton<MapData>(),
                    //bodyFromEntity = GetComponentDataFromEntity<MapBody>(true),
                    CommandBuffer = ecbSystem.CreateCommandBuffer().ToConcurrent()
                };


                var handle = job.Schedule(query, inputDeps);

                ecbSystem.AddJobHandleForProducer(handle);
                return handle;

            }
            [BurstCompile]
            struct FindPathJob : IJobForEachWithEntity_ECC<MapBody, MapBodyTranslation>
            {
                [ReadOnly]
                public MapData mapData;
                [ReadOnly]
                [DeallocateOnJobCompletion]
                public NativeArray<Entity> bodies;

                public EntityCommandBuffer.Concurrent CommandBuffer;
                public void Execute(Entity entity, int index, ref MapBody body, ref MapBodyTranslation translation)
                {
                    if (!body.point.Equals(translation.point))
                    {


                        NativeList<Node> history = new NativeList<Node>(Allocator.Temp);
                        NativeList<Node> frontier = new NativeList<Node>(Allocator.Temp);
                        if (!mapData.map.Value.GetTile(translation.point, out BlittableTile tile) || tile.Inaccessible)
                            return;
                        var current = new Node
                        {
                            point = body.point,
                            distanceToDestination = Distance(body.point, translation.point),
                            previousIndex = -1,
                            index = 0
                        };
                        int historyIndex = 1;
                        history.Add(current);
                        NativeList<Point> pointBuffer = new NativeList<Point>(Allocator.Temp);

                        int removeIndex;
                        int iteration = 0;
                        Node node;
                        while (!current.point.Equals(translation.point))
                        {
                            if (pointBuffer.IsCreated)
                                pointBuffer.Dispose();
                            pointBuffer = current.point.Expand(ref mapData.map.Value, 1, PathExpandPattern.SQUARE, Allocator.Temp);




                            for (int i = 0; i < history.Length; i++)
                            {
                                removeIndex = pointBuffer.IndexOf(history[i].point);
                                if (removeIndex >= 0)
                                    pointBuffer.RemoveAtSwapBack(removeIndex);
                            }

                                                    for (int i = 0; i < bodies.Length; i++)
                                                    {
                                                        removeIndex = pointBuffer.IndexOf(bodyFromEntity[bodies[i]].point);
                                                        if (removeIndex >= 0)
                                                            pointBuffer.RemoveAtSwapBack(removeIndex);
                                                    } 

                            for (int i = 0; i < pointBuffer.Length; i++)
                            {
                                node = new Node
                                {
                                    point = pointBuffer[i],
                                    distanceToDestination = Distance(pointBuffer[i], translation.point),
                                    distanceFromExpansion = Distance(current.point, pointBuffer[i]),
                                    previousIndex = current.index,
                                    index = historyIndex++
                                };
                                frontier.Add(node);
                                history.Add(node);
                            }
                            NativeSortExtension.Sort(frontier);
                            if (frontier.Length > 0)
                            {
                                current = frontier[0];
                                frontier.RemoveAtSwapBack(0);
                            }
                            else
                                break;
                        }
                        if (current.point.Equals(translation.point))
                        {
                            DynamicBuffer<MapBodyTranslationPoint> points = CommandBuffer.AddBuffer<MapBodyTranslationPoint>(index, entity);
                            while (current.previousIndex > 0)
                            {

                                points.Insert(0, new MapBodyTranslationPoint
                                {
                                    point = current.point,
                                    order = current.index
                                });
                                current = history[current.previousIndex];
                                iteration++;
                            }
                        }
                        CommandBuffer.RemoveComponent<MapBodyTranslation>(index, entity);
                        if (pointBuffer.IsCreated)
                            pointBuffer.Dispose();
                    }
                    else
                    {
                        CommandBuffer.RemoveComponent<MapBodyTranslation>(index, entity);
                    }





                }

                private int Distance(Point point, Point destination)
                {
                    return math.abs(point.x - destination.x) + math.abs(point.y - destination.y);
                }
                struct Node : IEquatable<Point>, IComparable<Node>
                {
                    public Point point;
                    public float distanceToDestination;
                    public int distanceFromExpansion;
                    public int previousIndex;
                    public int index;

                    public int CompareTo(Node other)
                    {
                        int compare = distanceToDestination.CompareTo(other.distanceToDestination);
                        return compare != 0 ? compare : other.distanceFromExpansion.CompareTo(distanceFromExpansion);
                    }

                    public bool Equals(Point other)
                    {
                        return point.Equals(other);
                    }
                }
            }

        }



        [UpdateInGroup(typeof(MapSystemGroup))]
        public class MapBodyToWorldSystem : JobComponentSystem
        {
            EntityQuery query;
            protected override void OnCreate()
            {

                query = EntityManager.CreateEntityQuery(new EntityQueryDesc
                {
                    All = new ComponentType[] { ComponentType.ReadOnly<MapBody>(), typeof(Translation) }
                });
                RequireForUpdate(query);
                RequireSingletonForUpdate<MapData>();
            }

            protected override JobHandle OnUpdate(JobHandle inputDeps)
            {

                var job = new ToWorldJob
                {
                    bodyTranslationPointsFromEntity = GetBufferFromEntity<MapBodyTranslationPoint>(),
                    mapData = GetSingleton<MapData>(),
                    meshData = GetArchetypeChunkSharedComponentType<RenderMesh>()
                };

                return job.Run(query, inputDeps);
            }

            struct ToWorldJob : IJobForEachWithEntity_ECC<MapBody, Translation>
            {
                [ReadOnly]
                public BufferFromEntity<MapBodyTranslationPoint> bodyTranslationPointsFromEntity;

                [ReadOnly]
                public MapData mapData;
                [ReadOnly]
                public ArchetypeChunkSharedComponentType<RenderMesh> meshData;

                public void Execute([ReadOnly] Entity entity, int index, [ReadOnly] ref MapBody body, ref Translation translation)
                {


                    if (bodyTranslationPointsFromEntity.Exists(entity))
                    {
                        DynamicBuffer<MapBodyTranslationPoint> path = bodyTranslationPointsFromEntity[entity];
                        if (path.Length > 0)
                        {
                            Point p = path[0].point;
                            Point dir = p - body.point;
                            translation.Value = new float3(body.point.x + dir.x * path[0].completion + 0.5f, 0.6f, body.point.y + dir.y * path[0].completion + 0.5f);

                        }
                        else
                        {
                            translation.Value = new float3(body.point.x + 0.5f, 0.6f, body.point.y + 0.5f);
                        }
                    }
                    else
                    {
                        translation.Value = new float3(body.point.x + 0.5f, 0.6f, body.point.y + 0.5f);
                    }



                }
            }
        } */

            [UpdateInGroup(typeof(MapSystemGroup))]
            [UpdateBefore(typeof(MapBodyPathFindingSystem))]
            public class MapBodyMovementSystem : JobComponentSystem
            {
                EntityQuery query;

                private EntityCommandBufferSystem ecbSystem;
                protected override void OnCreate()
                {

                    query = EntityManager.CreateEntityQuery(typeof(MapBodyTranslationPoint), typeof(MapBody));
                    ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
                    RequireForUpdate(query);
                    RequireSingletonForUpdate<MapData>();
                }
                protected override JobHandle OnUpdate(JobHandle inputDeps)
                {
                    MoveJob job = new MoveJob
                    {
                        deltaTime = World.Time.DeltaTime,
                        CommandBuffer = ecbSystem.CreateCommandBuffer().ToConcurrent()
                    };
                    var handle = job.Schedule(query, inputDeps);
                    ecbSystem.AddJobHandleForProducer(handle);
                    return handle;
                }
                private struct MoveJob : IJobForEachWithEntity_EBC<MapBodyTranslationPoint, MapBody>
                {
                    [ReadOnly]
                    public float deltaTime;

                    public EntityCommandBuffer.Concurrent CommandBuffer;
                    public void Execute(Entity entity, int index, DynamicBuffer<MapBodyTranslationPoint> tiles, ref MapBody body)
                    {
                        float distance = body.speed * deltaTime;
                        MapBodyTranslationPoint mapBodyTranslationPoint;
                        while (distance > 0 && tiles.Length > 0)
                        {
                            mapBodyTranslationPoint = tiles[0];
                            float newDistance = mapBodyTranslationPoint.completion + distance;


                            if (newDistance >= 1)
                            {
                                distance -= 1 - mapBodyTranslationPoint.completion;
                                body.point = tiles[0].point;
                                tiles.RemoveAt(0);
                            }
                            else
                            {
                                mapBodyTranslationPoint.completion = newDistance;
                                tiles[0] = mapBodyTranslationPoint;
                                distance = 0;
                            }
                        }
                    }
                }
            }

    /// <summary>
    /// Used for determining paths from MapBodyTranslation Components.
    /// </summary>
    [UpdateInGroup(typeof(MapSystemGroup))]

    public class MapBodyPathFindingSystem : JobComponentSystem
    {
        private EntityQuery query;
        private EntityCommandBufferSystem ecbSystem;
        protected override void OnCreate()
        {
            query = EntityManager.CreateEntityQuery(typeof(MapBodyTranslation), typeof(MapBody));
            RequireForUpdate(query);
            RequireSingletonForUpdate<MapData>();
        }
        protected override void OnStartRunning()
        {
            ecbSystem = World.GetExistingSystem<BattleSimulationEntityCommandBufferSystem>();
            Debug.Log(ecbSystem);
        }
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            var job = new FindPathJob
            {
                bodies = GetEntityQuery(typeof(MapBody)).ToEntityArray(Allocator.TempJob),
                mapData = GetSingleton<MapData>(),
                //bodyFromEntity = GetComponentDataFromEntity<MapBody>(true),
                CommandBuffer = ecbSystem.CreateCommandBuffer().ToConcurrent()
            };


            var handle = job.Schedule(query, inputDeps);

            ecbSystem.AddJobHandleForProducer(handle);
            return handle;

        }
        [BurstCompile]
        struct FindPathJob : IJobForEachWithEntity_ECC<MapBody, MapBodyTranslation>
        {
            [ReadOnly]
            public MapData mapData;
            [ReadOnly]
            [DeallocateOnJobCompletion]
            public NativeArray<Entity> bodies;

            public EntityCommandBuffer.Concurrent CommandBuffer;
            public void Execute(Entity entity, int index, ref MapBody body, ref MapBodyTranslation translation)
            {
                if (!body.point.Equals(translation.point))
                {


                    NativeList<Node> history = new NativeList<Node>(Allocator.Temp);
                    NativeList<Node> frontier = new NativeList<Node>(Allocator.Temp);
                    if (!mapData.map.Value.GetTile(translation.point, out BlittableTile tile) || tile.Inaccessible)
                        return;
                    var current = new Node
                    {
                        point = body.point,
                        distanceToDestination = Distance(body.point, translation.point),
                        previousIndex = -1,
                        index = 0
                    };
                    int historyIndex = 1;
                    history.Add(current);
                    NativeList<Point> pointBuffer = new NativeList<Point>(Allocator.Temp);

                    int removeIndex;
                    int iteration = 0;
                    Node node;
                    while (!current.point.Equals(translation.point))
                    {
                        if (pointBuffer.IsCreated)
                            pointBuffer.Dispose();
                        pointBuffer = current.point.Expand(ref mapData.map.Value, 1, PathExpandPattern.SQUARE, Allocator.Temp);




                        for (int i = 0; i < history.Length; i++)
                        {
                            removeIndex = pointBuffer.IndexOf(history[i].point);
                            if (removeIndex >= 0)
                                pointBuffer.RemoveAtSwapBack(removeIndex);
                        }

 /*                        for (int i = 0; i < bodies.Length; i++)
                        {
                            removeIndex = pointBuffer.IndexOf(bodyFromEntity[bodies[i]].point);
                            if (removeIndex >= 0)
                                pointBuffer.RemoveAtSwapBack(removeIndex);
                        } */

                        for (int i = 0; i < pointBuffer.Length; i++)
                        {
                            node = new Node
                            {
                                point = pointBuffer[i],
                                distanceToDestination = Distance(pointBuffer[i], translation.point),
                                distanceFromExpansion = Distance(current.point, pointBuffer[i]),
                                previousIndex = current.index,
                                index = historyIndex++
                            };
                            frontier.Add(node);
                            history.Add(node);
                        }
                        NativeSortExtension.Sort(frontier);
                        if (frontier.Length > 0)
                        {
                            current = frontier[0];
                            frontier.RemoveAtSwapBack(0);
                        }
                        else
                            break;
                    }
                    if (current.point.Equals(translation.point))
                    {
                        DynamicBuffer<MapBodyTranslationPoint> points = CommandBuffer.AddBuffer<MapBodyTranslationPoint>(index, entity);
                        while (current.previousIndex > 0)
                        {

                            points.Insert(0, new MapBodyTranslationPoint
                            {
                                point = current.point,
                                order = current.index
                            });
                            current = history[current.previousIndex];
                            iteration++;
                        }
                    }
                    CommandBuffer.RemoveComponent<MapBodyTranslation>(index, entity);
                    if (pointBuffer.IsCreated)
                        pointBuffer.Dispose();
                }
                else
                {
                    CommandBuffer.RemoveComponent<MapBodyTranslation>(index, entity);
                }





            }

            private int Distance(Point point, Point destination)
            {
                return math.abs(point.x - destination.x) + math.abs(point.y - destination.y);
            }
            struct Node : IEquatable<Point>, IComparable<Node>
            {
                public Point point;
                public float distanceToDestination;
                public int distanceFromExpansion;
                public int previousIndex;
                public int index;

                public int CompareTo(Node other)
                {
                    int compare = distanceToDestination.CompareTo(other.distanceToDestination);
                    return compare != 0 ? compare : other.distanceFromExpansion.CompareTo(distanceFromExpansion);
                }

                public bool Equals(Point other)
                {
                    return point.Equals(other);
                }
            }
        }

    }

    public class MapBodyToWorld : ComponentSystem
    {
        EntityQuery query;

        protected override void OnCreate()
        {

            query = EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<MapBody>(), typeof(Translation) }
            });
            RequireForUpdate(query);
            RequireSingletonForUpdate<MapData>();
            RequireSingletonForUpdate<MapRenderData>();


        }
        protected override void OnUpdate()
        {
            MapData mapData = GetSingleton<MapData>();
            MapRenderData renderData = GetSingleton<MapRenderData>();
            BufferFromEntity<MapBodyTranslationPoint> bodyTranslationPointsFromEntity = GetBufferFromEntity<MapBodyTranslationPoint>();
            Entities.With(query).ForEach((Entity entity, ref MapBody body, ref Translation translation) =>
            {
                float verticalOffset = EntityManager.HasComponent<RenderMesh>(entity) ? EntityManager.GetSharedComponentData<RenderMesh>(entity).mesh.bounds.extents.y : 0f;
                if (bodyTranslationPointsFromEntity.Exists(entity))
                {
                    DynamicBuffer<MapBodyTranslationPoint> path = bodyTranslationPointsFromEntity[entity];
                    if (path.Length > 0)
                    {
                        Point p = path[0].point;
                        Point dir = p - body.point;
                        translation.Value = new float3(body.point.x + dir.x * path[0].completion + renderData.tileSize / 2, (mapData.map.Value[body.point].Elevation * renderData.elevationStep) + verticalOffset, body.point.y + dir.y * path[0].completion + renderData.tileSize / 2);
                    }
                    else
                    {
                        translation.Value = new float3(body.point.x + renderData.tileSize / 2, (mapData.map.Value[body.point].Elevation * renderData.elevationStep) + verticalOffset, body.point.y + renderData.tileSize / 2);
                    }
                }
                else
                {
                    translation.Value = new float3(body.point.x + renderData.tileSize / 2, (mapData.map.Value[body.point].Elevation * renderData.elevationStep) + verticalOffset, body.point.y + renderData.tileSize / 2);
                }
            });

        }
    }
}