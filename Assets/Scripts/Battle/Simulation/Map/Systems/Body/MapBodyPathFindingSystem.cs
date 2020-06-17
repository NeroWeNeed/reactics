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
                    var destination = info.destination;
                    frontier.Clear();
                    nodeInfo.Clear();
                    var initialNode = new Node(start, start, 0, 0);
                    frontier.Add(initialNode);
                    nodeInfo[start] = initialNode;
                    bool found = false;
                    bool canReachDestination;

                    while (frontier.Length > 0)
                    {
                        var step = frontier.Pop();
                        if (step.point.Equals(info.destination))
                        {
                            found = true;
                            break;
                        }


                        canReachDestination = ExpandNode(entity, step.point, start, destination, info, mapData, collisionState, nodes);
                        if (!canReachDestination)
                        {
                            if (info.routeClosest && !step.point.Equals(start))
                            {
                                destination = step.point;
                                found = true;
                            }
                            break;
                        }
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
                        var current = destination;
                        while (!current.Equals(nodeInfo[current].previous))
                        {
                            buffer.Insert(0, new MapBodyPathFindingRoute(current, nodeInfo[current].previous, info.speed));
                            current = nodeInfo[current].previous;
                        }
                    }


                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="point"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <param name="info"></param>
        /// <param name="mapData"></param>
        /// <param name="collisionState"></param>
        /// <param name="nodes"></param>
        /// <returns>True if the destination was not found or the destination was found but is able to be traveled to. False otherwise</returns>
        private static bool ExpandNode(Entity entity, Point point, Point origin, Point destination, FindingPathInfo info, MapData mapData, MapCollisionState collisionState, NativeArray<Node> nodes)
        {
            int index = 0;
            bool canReachDestination = true;
            for (sbyte x = -1; x <= 1; x++)
            {
                for (sbyte y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    if (!point.TryShift(x, y, mapData.Width, mapData.Length, out Point target))
                        nodes[index++] = default;
                    else if (
                        (collisionState.value.TryGetValue(target, out Entity collision) && !collision.Equals(entity)) ||
                        (info.maxElevationDifference >= 0 && info.maxElevationDifference < math.abs(mapData.GetTile(origin).Elevation - mapData.GetTile(target).Elevation)) ||
                        mapData.GetTile(target).Inaccessible ||
                        (x != 0 && y != 0 && (mapData.GetTile(point.Shift(x, 0)).Inaccessible || mapData.GetTile(point.Shift(0, y)).Inaccessible))
                        )
                    {
                        nodes[index++] = default;
                        if (target.Equals(destination))
                            canReachDestination = false;
                    }
                    else
                    {
                        nodes[index++] = new Node(target, point, target.Distance(origin), target.Distance(destination));
                    }
                }
            }
            return canReachDestination;
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
                var fcost = gCost + hCost;
                var otherFcost = other.gCost + other.hCost;
                var comparison = fcost.CompareTo(otherFcost);
                if (comparison != 0)
                    return comparison;
                else
                {
                    comparison = hCost.CompareTo(other.hCost);
                    if (comparison != 0)
                        return comparison;
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

            public override string ToString()
            {
                return $"Node(Point={point},PreviousPoint={previous},GCost={gCost},HCost={hCost})";
            }
        }
    }
}