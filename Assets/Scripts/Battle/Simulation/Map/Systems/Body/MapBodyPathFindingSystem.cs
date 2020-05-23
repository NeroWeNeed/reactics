using System;
using System.Runtime.InteropServices;
using Reactics.Commons.Collections;
using Reactics.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Battle.Map
{
    [UpdateInGroup(typeof(MapBodyManagementSystemGroup))]
    [UpdateAfter(typeof(MapBodyCollisionStateSystem))]
    public class MapBodyPathFindingSystem : SystemBase
    {
        private EntityQuery query;
        private EntityCommandBufferSystem commandBufferSystem;
        protected override void OnCreate()
        {

            query = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { ComponentType.ReadOnly<MapBody>(), ComponentType.ReadOnly<MapElement>(), ComponentType.ReadOnly<FindingPathInfo>() },
                None = new ComponentType[] { typeof(MapBodyPathFindingRoute) }
            });
            RequireForUpdate(query);
            commandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {


            Dependency = new PathFindingJob
            {
                entityType = GetArchetypeChunkEntityType(),
                collisionStateType = GetComponentDataFromEntity<MapCollisionState>(true),
                mapDataType = GetComponentDataFromEntity<MapData>(true),
                mapBodyType = GetArchetypeChunkComponentType<MapBody>(true),
                mapElementType = GetArchetypeChunkComponentType<MapElement>(true),
                findingPathInfoType = GetArchetypeChunkComponentType<FindingPathInfo>(true),
                ecb = commandBufferSystem.CreateCommandBuffer().ToConcurrent()

            }.Schedule(query, Dependency);

            commandBufferSystem.AddJobHandleForProducer(Dependency);

        }

        public struct PathFindingJob : IJobChunk
        {

            [ReadOnly]
            public ArchetypeChunkEntityType entityType;
            [ReadOnly]
            public ComponentDataFromEntity<MapCollisionState> collisionStateType;
            [ReadOnly]
            public ComponentDataFromEntity<MapData> mapDataType;
            [ReadOnly]
            public ArchetypeChunkComponentType<MapBody> mapBodyType;
            [ReadOnly]
            public ArchetypeChunkComponentType<MapElement> mapElementType;
            [ReadOnly]
            public ArchetypeChunkComponentType<FindingPathInfo> findingPathInfoType;

            public EntityCommandBuffer.Concurrent ecb;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {

                var entities = chunk.GetNativeArray(entityType);
                var findingPathInfos = chunk.GetNativeArray(findingPathInfoType);
                var mapBodies = chunk.GetNativeArray(mapBodyType);
                var mapElements = chunk.GetNativeArray(mapElementType);
                NativeSortedSet<Node> frontier = new NativeSortedSet<Node>(8);
                NativeArray<Node> nodes = new NativeArray<Node>(8, Allocator.Temp);
                NativeHashMap<Point, Node> nodeInfo = new NativeHashMap<Point, Node>(8, Allocator.Temp);
                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    var start = mapBodies[i].point;
                    var mapEntity = mapElements[i].value;
                    var mapData = mapDataType[mapEntity];
                    var collisionState = collisionStateType[mapEntity];
                    var info = findingPathInfos[i];


                    frontier.Clear();
                    nodeInfo.Clear();
                    var initialNode = new Node(start, start, 0, 0);
                    frontier.Add(initialNode);
                    nodeInfo[start] = initialNode;
                    bool found = false;
                    
                    while (frontier.Length > 0)
                    {

                        var step = frontier.Pop();
                        
                        if (step.point.Equals(info.destination))
                        {
                            found = true;
                            break;
                        }


                        ExpandNode(entity, step.point, start, info.destination, info, mapData, collisionState, nodes);
                        for (int j = 0; j < nodes.Length; j++)
                        {
                            var node = nodes[j];
                            
                            if (node.Equals(default))
                                continue;
                            if (nodeInfo.TryGetValue(node.point, out Node oldNode))
                            {
                                if (oldNode.CompareTo(node) > 0)
                                {
                                    nodeInfo[node.point] = node;
                                    if (frontier.Contains(oldNode))
                                    {
                                        frontier.Remove(oldNode);
                                        frontier.Add(node);
                                    }
                                }
                            }
                            else
                            {
                                nodeInfo[node.point] = node;
                                frontier.Add(node);
                            }

                        }
                    }
                    if (found)
                    {
                        
                        var buffer = ecb.AddBuffer<MapBodyPathFindingRoute>(chunkIndex, entity);
                        var current = info.destination;
                        while (!current.Equals(nodeInfo[current].previous))
                        {
                            buffer.Insert(0, new MapBodyPathFindingRoute(current, nodeInfo[current].previous, info.speed));
                            current = nodeInfo[current].previous;
                        }
                    }


                }

            }
        }

        private static void ExpandNode(Entity entity, Point point, Point origin, Point destination, FindingPathInfo info, MapData mapData, MapCollisionState collisionState, NativeArray<Node> nodes)
        {
            int index = 0;
            for (sbyte x = -1; x <= 1; x++)
            {
                for (sbyte y = -1; y <= 1; y++)
                {
                    if ((x == 0 && y == 0) ||
                    !point.TryShift(x, y, mapData.Width, mapData.Length, out Point target) ||
                    collisionState.value.ContainsKey(target) ||
                    (info.maxElevationDifference >= 0 && info.maxElevationDifference < math.abs(mapData.GetTile(origin).Elevation - mapData.GetTile(target).Elevation)) ||
                    mapData.GetTile(target).Inaccessible ||
                    (x != 0 && y != 0 && (mapData.GetTile(point.Shift(x, 0)).Inaccessible || mapData.GetTile(point.Shift(0, y)).Inaccessible))
                    )
                        nodes[index] = default;
                    else {

                        nodes[index] = new Node(target, point, target.Distance(origin), target.Distance(destination));
                    }
                }
                index++;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Node : IComparable<Node>, IEquatable<Node>
        {
            public Point point;

            public Point previous;
            public float gCost, hCost;
            public BlittableBool valid;

            public Node(Point point, Point previous, float gCost, float hCost)
            {
                this.point = point;
                this.previous = previous;
                this.gCost = gCost;
                this.hCost = hCost;
                valid = true;
            }

            public int CompareTo(Node other)
            {
                var f1 = gCost + hCost;
                var f2 = other.gCost + other.hCost;
                var fCompare = f1.CompareTo(f2);
                if (fCompare != 0)
                    return fCompare;
                else
                {
                    var hCompare = hCost.CompareTo(other.hCost);
                    if (hCompare != 0)
                        return hCompare;
                    return gCost.CompareTo(other.gCost);
                }
            }


            public bool Equals(Node other)
            {
                return point.Equals(other.point) &&
       previous.Equals(other.previous) &&
       gCost == other.gCost &&
       hCost == other.hCost &&
       valid == other.valid;
            }
        }
    }
}