/*using Unity.Entities; broken at the moment
using Unity.Jobs;
using Unity.Collections;
using Reactics.Battle;
using System;
using Reactics.Commons;
using Reactics.Battle.Map;
using Reactics.Commons.Collections;

/*
highlighting logic
line of sight blockage *only* for projectiles.
Maybe not even that? We coul just... have it hit the inaccessible tile and that's that. hrm. nahhh that seems kinda lame... and like, unintuitive.
we want to at least not let them try to shoot behind walls. that'd be dumb as hell...
cost for movement, maybe? ask. for now it's just inaccessible stuff. bleh.
*&*&
[UpdateInGroup(typeof(RenderingSystemGroup))]
[UpdateAfter(typeof(CursorHighlightSystem))]
public class UnitTileHighlightSystem : SystemBase
{
    private EntityQuery query;
    
    public struct HighlightNode : IComparable<HighlightNode>, IEquatable<HighlightNode>
    {
        public Point point;
        public ushort cost;

        public HighlightNode(Point point, ushort cost)
        {
            this.point = point;
            this.cost = cost;
        }

        public int CompareTo(HighlightNode other)
        {
            return 0; //not sure why this is necessary
        }

        public bool Equals(HighlightNode other)
        {
            if (this.point.x == other.point.x &&
                this.point.y == other.point.y &&
                this.cost == other.cost) //TODO: get rid of cost comparison WITHOUT higher movements exploding please~
                return true;
            return false;
        }
    }
    
    protected override void OnCreate()
    {
        //query all map bodies to see if we are colliding with one of them...
        query = GetEntityQuery(typeof(MapBody), ComponentType.ReadOnly<MapElement>());
        RequireForUpdate(query);
        //mapBodiesQuery = GetEntityQuery(typeof(MapBody));//, 
           //ComponentType.ReadOnly<SomeType>());
    }

    protected override void OnUpdate() 
    {
        BufferFromEntity<HighlightTile> highlightTilesFromEntity = GetBufferFromEntity<HighlightTile>(false);
        UnitManagerData unitManagerData = GetSingleton<UnitManagerData>();
        //var mapBodyData = mapBodiesQuery.ToComponentDataArray<MapBody>(Allocator.TempJob);
        MapData mapData = GetSingleton<MapData>();
        //we could do a change filter with some tag component that gets attached to moving/selected mapbodies, maybe. not the worst idea, yeah?
        Entities/*.WithChangeFilter<MoveTilesTag>().WithAll<MoveTilesTag>()*&*&.ForEach((Entity entity, ref MapBody mapBody, in UnitStatData unitData, in MapElement mapElement) =>
        {
            var move = unitData.Movement;
            DynamicBuffer<HighlightTile> highlightTiles = highlightTilesFromEntity[entity];
            if (unitManagerData.selectedUnit == entity && highlightTilesFromEntity.HasComponent(entity))
            {
                highlightTiles.Clear();
                Point originPoint = new Point(mapBody.point.x, mapBody.point.y);

                //Movement tiles
                if (unitManagerData.commanding)
                {
                    //highlight tiles
                    NativeList<HighlightNode> visited = new NativeList<HighlightNode>(Allocator.Temp);
                    NativeSortedSet<HighlightNode> toVisit = new NativeSortedSet<HighlightNode>(8);
                    HighlightNode origin = new HighlightNode(originPoint, 0);
                    toVisit.Add(origin);
                    visited.Add(origin);
                    //Thing closest = frontier.Peek();
                    //NativeArray<Thing> expansion = new NativeArray<Thing>(4, Allocator.Temp);

                    HighlightNode leftNeighbor = new HighlightNode(new Point(0,0), 0);
                    HighlightNode rightNeighbor = new HighlightNode(new Point(0,0), 0);
                    HighlightNode upNeighbor = new HighlightNode(new Point(0,0), 0);
                    HighlightNode downNeighbor = new HighlightNode(new Point(0,0), 0);

                    while (toVisit.Length > 0)
                    {
                        var current = toVisit.Pop();
                        visited.Add(current);
                        highlightTiles.Add(new HighlightTile { point = new Point(current.point.x, current.point.y), state = (ushort)MapLayer.PlayerMove });

                        //check all four directions
                        //pretend we're getting the cost of the tile from somewhere, and putting it in the cost variable for each direction.
                        //That is to say, when ti says current.cost + 1, that 1 should be calculated from a tile effect if applicable.
                        if (current.point.x > 0)
                        {
                            leftNeighbor.point = new Point(current.point.x-1, current.point.y);
                            leftNeighbor.cost = (ushort)(current.cost + 1);
                        }
                        
                        if (current.point.x < mapData.Length - 1)
                        {
                            rightNeighbor.point = new Point(current.point.x+1, current.point.y);
                            rightNeighbor.cost = (ushort)(current.cost + 1);
                        }
                        
                        if (current.point.y > 0)
                        {
                            downNeighbor.point = new Point(current.point.x, current.point.y-1);
                            downNeighbor.cost = (ushort)(current.cost + 1);
                        }

                        if (current.point.y < mapData.Width - 1)
                        {
                            upNeighbor.point = new Point(current.point.x, current.point.y+1);
                            upNeighbor.cost = (ushort)(current.cost + 1);
                        }

                        if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(leftNeighbor.point, out Entity leftCollidableEntity))
                            leftNeighbor.cost = 0;
                        if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(leftNeighbor.point, out Entity rightCollidableEntity))
                            rightNeighbor.cost = 0;
                        if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(leftNeighbor.point, out Entity upCollidableEntity))
                            upNeighbor.cost = 0;
                        if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(leftNeighbor.point, out Entity downCollidableEntity))
                            downNeighbor.cost = 0;

                        //if (current.point.x - 1 > 0 && !visited.Contains(new Thing(new Point(current.point.x -1, current.point.y), (ushort)(current.cost + cost))))
                        //TODO: Fix duplicates. still has plenty because cost is different. annoying.
                        //(could we somehow have visited just be a list of points...? we don't really need to have the cost there too, do we?)
                        if (!visited.Contains(leftNeighbor) && leftNeighbor.cost > 0 && leftNeighbor.cost < move && !mapData.GetTile(leftNeighbor.point.x, leftNeighbor.point.y).Inaccessible)
                        {
                            toVisit.Add(leftNeighbor);
                        }
                        if (!visited.Contains(rightNeighbor) && rightNeighbor.cost > 0 && rightNeighbor.cost < move && !mapData.GetTile(rightNeighbor.point.x, rightNeighbor.point.y).Inaccessible)
                        {
                            toVisit.Add(rightNeighbor);
                        }
                        if (!visited.Contains(upNeighbor) && upNeighbor.cost > 0 && upNeighbor.cost < move && !mapData.GetTile(upNeighbor.point.x, upNeighbor.point.y).Inaccessible)
                        {
                            toVisit.Add(upNeighbor);
                        }
                        if (!visited.Contains(downNeighbor) && downNeighbor.cost > 0 && downNeighbor.cost < move && !mapData.GetTile(downNeighbor.point.x, downNeighbor.point.y).Inaccessible)
                        {
                            toVisit.Add(downNeighbor);
                        }

                        leftNeighbor.cost = 0;
                        rightNeighbor.cost = 0;
                        downNeighbor.cost = 0;
                        upNeighbor.cost = 0;
                    }
                    visited.Dispose();
                    toVisit.Dispose();
                }

                //Action tiles
                if (unitManagerData.effectSelected)
                {
                    highlightTiles.Clear();//delet this
                    MapLayer highlightLayer = MapLayer.Base;
                    //NativeList<Point> endPoints = new NativeList<Point>(Allocator.Temp);
                    NativeList<Point> pathToTargetTile = new NativeList<Point>(Allocator.Temp);
                    NativeList<Point> fullEffectHighlightRange = new NativeList<Point>(Allocator.Temp);
                    
                    if (unitManagerData.effect.harmful)
                        highlightLayer = MapLayer.PlayerAttack;
                    else
                        highlightLayer = MapLayer.PlayerSupport;
                    originPoint = unitManagerData.moveTile;
                    for (int i = 0; i <= unitManagerData.effect.range; i++)
                    {
                        for (int j = 0; j <= unitManagerData.effect.range; j++)
                        {
                            if (i + j <= unitManagerData.effect.range)
                            {
                                //+i and +j
                                Point nextPoint = new Point(originPoint.x + i, originPoint.y + j);
                                if (nextPoint.x < mapData.Length && nextPoint.y < mapData.Width && !mapData.GetTile(nextPoint).Inaccessible)
                                {
                                    fullEffectHighlightRange.Add(nextPoint);
                                    /*if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                    {
                                        endPoints.Add(nextPoint);
                                    }*&*&
                                }

                                //-i and -j
                                if (originPoint.x - i >= 0 && originPoint.y - j >= 0)
                                {
                                    nextPoint = new Point(originPoint.x - i, originPoint.y - j);
                                    if ((i != 0 || j != 0) && !mapData.GetTile(nextPoint).Inaccessible)
                                    {
                                        fullEffectHighlightRange.Add(nextPoint);
                                        /*if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                        {
                                            endPoints.Add(nextPoint);
                                        }*&*&
                                    }
                                }
                                if (i != 0 && j != 0)
                                {
                                    //+i and -j
                                    if (originPoint.y - j >= 0)
                                    {
                                        nextPoint = new Point(originPoint.x + i, originPoint.y - j);
                                        if (nextPoint.x < mapData.Length && nextPoint.y >= 0 && !mapData.GetTile(nextPoint).Inaccessible)
                                        {
                                            fullEffectHighlightRange.Add(nextPoint);
                                            /*if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                            {
                                                endPoints.Add(nextPoint);
                                            }*&*&
                                        }
                                    }

                                    //-i and +j
                                    if (originPoint.x - i >= 0)
                                    {
                                        nextPoint = new Point(originPoint.x - i, originPoint.y + j);
                                        if (nextPoint.x >= 0 && nextPoint.y < mapData.Width && !mapData.GetTile(nextPoint).Inaccessible)
                                        {
                                            fullEffectHighlightRange.Add(nextPoint);
                                            /*if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                            {
                                                endPoints.Add(nextPoint);
                                            }*&*&
                                        }
                                    }
                                }
                            }
                        }
                    }

                    for (int i = 0; i < fullEffectHighlightRange.Length; i++)
                    {
                        //XW Alg for each tile in the effect highlight range.
                        int startingIndex = pathToTargetTile.Length;
                        bool addTileToHighlightTiles = true;
                        originPoint.GetPathToTargetTile(ref pathToTargetTile, fullEffectHighlightRange[i], unitManagerData.effect.range, mapData.Length, mapData.Width, 0.1);
                        //normally we'd use calculatelos3 here but we cna just od it here with the new stuff right...?

                        if (pathToTargetTile.Length > 0)
                        {
                            for (var j = pathToTargetTile.Length - 1; j >= startingIndex; j--)
                            {
                                //Check for obstructions
                                if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(pathToTargetTile[j], out Entity collidableEntity))
                                {
                                    //this means there was an obstruction
                                    addTileToHighlightTiles = false;
                                    break;
                                }
                            }

                            if (addTileToHighlightTiles)
                                highlightTiles.Add(new HighlightTile{point = fullEffectHighlightRange[i], state = (ushort)highlightLayer});
                        }
                    }
                    pathToTargetTile.Clear();
                    fullEffectHighlightRange.Clear();
                    pathToTargetTile.Dispose();
                    fullEffectHighlightRange.Dispose();
                }
            }
        }).Schedule();
    }
}*/