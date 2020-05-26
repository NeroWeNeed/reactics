using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Reactics.Battle;
using System;
using Reactics.Commons;

/*
highlighting logic
line of sight blockage *only* for projectiles.
Maybe not even that? We coul just... have it hit the inaccessible tile and that's that. hrm. nahhh that seems kinda lame... and like, unintuitive.
we want to at least not let them try to shoot behind walls. that'd be dumb as hell...
cost for movement, maybe? ask. for now it's just inaccessible stuff. bleh.
*/
[UpdateInGroup(typeof(RenderingSystemGroup))]
[UpdateAfter(typeof(CursorHighlightSystem))]
public class UnitTileHighlightSystem : SystemBase
{
    public struct Thing : IComparable<Thing>, IEquatable<Thing>
    {
        public Point point;
        public ushort cost;

        public Thing(Point point, ushort cost)
        {
            this.point = point;
            this.cost = cost;
        }

        public int CompareTo(Thing other)
        {
            return 0; //not sure why this is necessary
        }

        public bool Equals(Thing other)
        {
            if (this.point.x == other.point.x &&
                this.point.y == other.point.y &&
                this.cost == other.cost) //TODO: get rid of cost comparison WITHOUT higher movements exploding please~
                return true;
            return false;
        }
    }
    
    private EntityQuery mapBodiesQuery;
    
    protected override void OnCreate()
    {
        //query all map bodies to see if we are colliding with one of them...
        mapBodiesQuery = GetEntityQuery(typeof(MapBody));//, 
           //ComponentType.ReadOnly<SomeType>());
    }

    protected override void OnUpdate() 
    {
        BufferFromEntity<HighlightTile> highlightTilesFromEntity = GetBufferFromEntity<HighlightTile>(false);
        UnitManagerData unitManagerData = GetSingleton<UnitManagerData>();
        var mapBodyData = mapBodiesQuery.ToComponentDataArray<MapBody>(Allocator.TempJob);
        MapData mapData = GetSingleton<MapData>();
        //we could do a change filter with some tag component that gets attached to moving/selected mapbodies, maybe. not the worst idea, yeah?
        Entities.WithChangeFilter<MoveTilesTag>().WithAll<MoveTilesTag>().ForEach((Entity entity, ref MapBody mapBody, in UnitData unitData) =>
        {
            var move = unitData.Movement();
            DynamicBuffer<HighlightTile> highlightTiles = highlightTilesFromEntity[entity];
            highlightTiles.Clear();

            if (unitManagerData.selectedUnit == entity && highlightTilesFromEntity.Exists(entity))
            {
                Point originPoint = new Point(mapBody.point.x, mapBody.point.y);
                NativeList<Point> mapBodyPoints = new NativeList<Point>(Allocator.Temp);
                NativeList<Point> obstructions = new NativeList<Point>(Allocator.Temp);
                for (int i = 0; i < mapBodyData.Length; i++)
                {
                    //if (originPoint.ComparePoints(mapBodyData[i].point)) remove when done testing~
                    //{
                        mapBodyPoints.Add(mapBodyData[i].point);
                        obstructions.Add(mapBodyData[i].point);
                    //}
                }

                //Movement tiles
                if (unitManagerData.commanding)
                {
                    //highlight tiles
                    NativeList<Thing> visited = new NativeList<Thing>(Allocator.Temp);
                    NativeHeap<Thing> toVisit = new NativeHeap<Thing>(Allocator.Temp);
                    Thing origin = new Thing(originPoint, 0);
                    toVisit.Add(origin);
                    visited.Add(origin);
                    Thing current;
                    //Thing closest = frontier.Peek();
                    //NativeArray<Thing> expansion = new NativeArray<Thing>(4, Allocator.Temp);

                    Thing leftNeighbor = new Thing(new Point(0,0), 0);
                    Thing rightNeighbor = new Thing(new Point(0,0), 0);
                    Thing upNeighbor = new Thing(new Point(0,0), 0);
                    Thing downNeighbor = new Thing(new Point(0,0), 0);

                    while (toVisit.Pop(out current))
                    {
                        visited.Add(current);
                        highlightTiles.Add(new HighlightTile { point = new Point(current.point.x, current.point.y), layer = MapLayer.PLAYER_MOVE });

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

                        for (int i = 0; i < mapBodyPoints.Length; i++)
                        {
                            //could make this better, maybe.
                            //just put these and inaccessibles in the same list, and check for those?
                            //but maybe then some dash mechanic would get fucked, or something, idk.
                            if (leftNeighbor.point.ComparePoints(mapBodyPoints[i]))
                                leftNeighbor.cost = 0;
                            if (rightNeighbor.point.ComparePoints(mapBodyPoints[i]))
                                rightNeighbor.cost = 0;
                            if (upNeighbor.point.ComparePoints(mapBodyPoints[i]))
                                upNeighbor.cost = 0;
                            if (downNeighbor.point.ComparePoints(mapBodyPoints[i]))
                                downNeighbor.cost = 0;
                        }

                        //if (current.point.x - 1 > 0 && !visited.Contains(new Thing(new Point(current.point.x -1, current.point.y), (ushort)(current.cost + cost))))
                        //TODO: Fix duplicates. still has plenty because cost is different. annoying.
                        //(could we somehow have visited just be a list of points...? we don't really need to have the cost there too, do we?)
                        if (!visited.Contains(leftNeighbor) && leftNeighbor.cost > 0 && leftNeighbor.cost < move && !mapData[leftNeighbor.point.x, leftNeighbor.point.y].inaccessible)
                        {
                            toVisit.Add(leftNeighbor);
                        }
                        if (!visited.Contains(rightNeighbor) && rightNeighbor.cost > 0 && rightNeighbor.cost < move && !mapData[rightNeighbor.point.x, rightNeighbor.point.y].inaccessible)
                        {
                            toVisit.Add(rightNeighbor);
                        }
                        if (!visited.Contains(upNeighbor) && upNeighbor.cost > 0 && upNeighbor.cost < move && !mapData[upNeighbor.point.x, upNeighbor.point.y].inaccessible)
                        {
                            toVisit.Add(upNeighbor);
                        }
                        if (!visited.Contains(downNeighbor) && downNeighbor.cost > 0 && downNeighbor.cost < move && !mapData[downNeighbor.point.x, downNeighbor.point.y].inaccessible)
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
                    mapBodyPoints.Dispose();
                }

                //Action tiles
                if (unitManagerData.effectSelected)
                {
                    MapLayer highlightLayer = MapLayer.BASE;
                    NativeList<Point> endPoints = new NativeList<Point>(Allocator.Temp);
                    NativeList<Point> fullPathToEndPoint = new NativeList<Point>(Allocator.Temp);
                    NativeList<Point> fullHighlightPath = new NativeList<Point>(Allocator.Temp);
                    NativeList<Point> fullHighlightRange = new NativeList<Point>(Allocator.Temp);
                    //originPoint.GetPathToTargetPoint(targetPoint, 0);
                    
                    if (unitManagerData.effect.harmful)
                        highlightLayer = MapLayer.PLAYER_ATTACK;
                    else
                        highlightLayer = MapLayer.PLAYER_SUPPORT;
                    originPoint = unitManagerData.moveTile;
                    for (int i = 0; i <= unitManagerData.effect.range; i++)
                    {
                        for (int j = 0; j <= unitManagerData.effect.range; j++)
                        {
                            if (i + j <= unitManagerData.effect.range)
                            {
                                //+i and +j
                                Point nextPoint = new Point(originPoint.x + i, originPoint.y + j);
                                if (nextPoint.x < mapData.Length && nextPoint.y < mapData.Width && !mapData[nextPoint].inaccessible)
                                {
                                    fullHighlightRange.Add(nextPoint);
                                    if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                    {
                                        endPoints.Add(nextPoint);
                                    }
                                }
                                else if (nextPoint.x < mapData.Length && nextPoint.y < mapData.Width && mapData[nextPoint].inaccessible)
                                {
                                    obstructions.Add(nextPoint);
                                }

                                //-i and -j
                                if (originPoint.x - i >= 0 && originPoint.y - j >= 0)
                                {
                                    nextPoint = new Point(originPoint.x - i, originPoint.y - j);
                                    if ((i != 0 || j != 0) && !mapData[nextPoint].inaccessible)
                                    {
                                        fullHighlightRange.Add(nextPoint);
                                        if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                        {
                                            endPoints.Add(nextPoint);
                                        }
                                    }
                                    else if ((i != 0 || j != 0) && mapData[nextPoint].inaccessible)
                                    {
                                        obstructions.Add(nextPoint);
                                    }
                                }
                                if (i != 0 && j != 0)
                                {
                                    //+i and -j
                                    if (originPoint.y - j >= 0)
                                    {
                                        nextPoint = new Point(originPoint.x + i, originPoint.y - j);
                                        if (nextPoint.x < mapData.Length && nextPoint.y >= 0 && !mapData[nextPoint].inaccessible)
                                        {
                                            fullHighlightRange.Add(nextPoint);
                                            if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                            {
                                                endPoints.Add(nextPoint);
                                            }
                                        }
                                        else if (nextPoint.x < mapData.Length && nextPoint.y >= 0 && mapData[nextPoint].inaccessible)
                                        {
                                            obstructions.Add(nextPoint);
                                        }
                                    }

                                    //-i and +j
                                    if (originPoint.x - i >= 0)
                                    {
                                        nextPoint = new Point(originPoint.x - i, originPoint.y + j);
                                        if (nextPoint.x >= 0 && nextPoint.y < mapData.Width && !mapData[nextPoint].inaccessible)
                                        {
                                            fullHighlightRange.Add(nextPoint);
                                            if (originPoint.IsEndpoint(nextPoint, unitManagerData.effect.range, mapData.Length, mapData.Width))
                                            {
                                                endPoints.Add(nextPoint);
                                            }
                                        }
                                        else if (nextPoint.x >= 0 && nextPoint.y < mapData.Width && mapData[nextPoint].inaccessible)
                                        {
                                            obstructions.Add(new Point(originPoint.x - i, originPoint.y + j));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //For each end point, calc the tiles we get to keep.
                    //TODO: If we're going to bresenham from every tile, we just need to do the visible calc for one tile...
                    //this means we can remove a for loop probably (either way we can get rid of one with exclusive so...)
                    NativeList<Point> removeTiles = new NativeList<Point>(Allocator.Temp);
                    for (int i = 0; i < fullHighlightRange.Length; i++)
                    {
                        //inclusive
                        /*//Gets the list from the origin to the end point.
                        originPoint.CalculateLOS(ref fullPathToEndPoint, fullHighlightRange[i], unitManagerData.effect.range, mapData.Length, mapData.Width);
                        //Adds to the actual list of tiles to highlight
                        if (fullPathToEndPoint.Length > 0)
                        {
                            originPoint.CalculateLOS3(ref fullPathToEndPoint, ref obstructions, ref highlightTiles, highlightLayer, fullHighlightRange[i],
                                                        unitManagerData.effect.range, mapData.Length, mapData.Width);
                            fullPathToEndPoint.Clear();
                        }*/
                        //exclusive p1
                        highlightTiles.Add(new HighlightTile{point = fullHighlightRange[i], layer = highlightLayer});
                    }
                    for (int i = 0; i < fullHighlightRange.Length; i++)
                    {
                        int startingIndex = fullPathToEndPoint.Length;
                        originPoint.CalculateLOSXW(ref fullPathToEndPoint, fullHighlightRange[i], unitManagerData.effect.range, mapData.Length, mapData.Width, 0.1);
                        originPoint.CalculateLOS3(ref fullPathToEndPoint, ref obstructions, ref removeTiles, highlightLayer, fullHighlightRange[i],
                                                        unitManagerData.effect.range, mapData.Length, mapData.Width, startingIndex);
                    }
                    for (int i = 0; i < highlightTiles.Length; i++)
                    {
                        for (int j = 0; j < removeTiles.Length; j++)
                        {
                            if (highlightTiles[i].point.ComparePoints(removeTiles[j]) && highlightTiles[i].layer == highlightLayer)
                            {
                                highlightTiles.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    fullPathToEndPoint.Clear();
                    fullHighlightRange.Clear();
                    obstructions.Clear();
                    removeTiles.Clear();
                    fullPathToEndPoint.Dispose();
                    fullHighlightRange.Dispose();
                    obstructions.Dispose();
                    removeTiles.Dispose();
                    //the problem then becomes... we have to check like, every point or something. blehhhh. for now let's just... do it. whatever.
                    //It is time for your CPU to be eliminated, fool.
                    /*NativeList<Point> tilesToHighlight = new NativeList<Point>(Allocator.Temp);
                    highlightTiles.Clear();
                    for (int i = 0; i < tilesToHighlight.Length; i++)
                    {
                        highlightTiles.Add((new HighlightTile { point = tilesToHighlight[i], layer = highlightLayer }));
                    }*/
                    //we now have every end point. for each one we need to calculate line of sight, and get the tiles that we're allowed to keep.

                    /*for (int i = 0; i < highlightTiles.Length; i++)
                    {
                        //removePoints.Add(calcPoints[i]);
                        //highlightTiles.RemoveAt(i);
                        //i--;
                        if (highlightTiles[i].layer != highlightLayer)
                            continue;
                            
                        originPoint.CalculateLOS(ref removePoints, highlightTiles[i].point, unitManagerData.effect.range, mapData.Length, mapData.Width);
                        if (removePoints.Length > 0)
                        {
                            highlightTiles.RemoveAt(i);
                            i--;
                            removePoints.Clear();
                        }
                        //for (int j = 0; j < calcPoints.Length; j++)
                        //{
                        //    if (highlightTiles[i].point.ComparePoints(calcPoints[j]))
                        //    {
                        //        highlightTiles.RemoveAt(i);
                        //        removePoints.Clear();
                        //        break;
                        //    }
                        //}
                    }*/
                    /*for (int i = 0; i < mapBodyPoints.Length; i++)
                    {
                        calcPoints.Add(mapBodyPoints[i]);
                    }*/

                    
                    /*if (removePoints.Length > 0)
                    {
                        for (int i = 0; i < highlightTiles.Length; i++)
                        {
                            for (int j = 0; j < removePoints.Length; j++)
                            {
                                if (highlightTiles[i].point.ComparePoints(removePoints[j]))
                                {
                                    highlightTiles.RemoveAt(i);
                                    //continue;
                                }
                            }
                        }
                    }*/
                }
            }
        }).WithoutBurst().WithDeallocateOnJobCompletion(mapBodyData).Schedule();
    }
}