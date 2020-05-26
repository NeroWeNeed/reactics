using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Reactics.Util;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Reactics.Battle
{
    public static class MapUtils
    {

        public static Point GetTilePoint(this ref MapData self, int index)
        {
            return new Point((ushort)(index % self.Width), (ushort)(index / self.Width));
        }
        public static Mesh GenerateMesh(this ref MapBlob map, float tileSize = 1f, float elevationStep = 0.25f)
        {

            int vertexCount = map.Width * map.Length * 4;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[map.Width * map.Length * 6];
            int x, y, index;
            for (y = 0; y < map.Length; y++)
            {
                for (x = 0; x < map.Width; x++)
                {
                    index = (y * map.Width + x) * 4;

                    vertices[index] = new Vector3(x * tileSize, map[x, y].Elevation * elevationStep, y * tileSize);
                    uv[index] = new Vector2((float)x / map.Width, (float)y / map.Length);
                    normals[index] = Vector3.up;

                    vertices[index + 1] = new Vector3((x + 1) * tileSize, map[x, y].Elevation * elevationStep, y * tileSize);
                    uv[index + 1] = new Vector2(((float)x + 1) / map.Width, (float)y / map.Length);
                    normals[index + 1] = Vector3.up;

                    vertices[index + 2] = new Vector3(x * tileSize, map[x, y].Elevation * elevationStep, (y + 1) * tileSize);
                    uv[index + 2] = new Vector2((float)x / map.Width, ((float)y + 1) / map.Length);
                    normals[index + 2] = Vector3.up;

                    vertices[index + 3] = new Vector3((x + 1) * tileSize, map[x, y].Elevation * elevationStep, (y + 1) * tileSize);
                    uv[index + 3] = new Vector2(((float)x + 1) / map.Width, ((float)y + 1) / map.Length);
                    normals[index + 3] = Vector3.up;
                }
            }
            for (y = 0; y < map.Length; y++)
            {
                for (x = 0; x < map.Width; x++)
                {
                    GenerateMeshTileTriangles(triangles, (y * map.Width + x) * 6, x, y, map.Width);
                }
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.subMeshCount = 2;
            mesh.normals = normals;
            return mesh;
        }
        public static Mesh GenerateMesh<T, S>(this IMap<T, S> map, float tileSize = 1f, float elevationStep = 0.25f) where T : IMapTile where S : IMapSpawnGroup
        {
            int vertexCount = map.Width * map.Length * 4;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[map.Width * map.Length * 6];
            int x, y, index;
            for (y = 0; y < map.Length; y++)
            {
                for (x = 0; x < map.Width; x++)
                {
                    index = (y * map.Width + x) * 4;

                    vertices[index] = new Vector3(x * tileSize, map[x, y].Elevation * elevationStep, y * tileSize);
                    uv[index] = new Vector2((float)x / map.Width, (float)y / map.Length);
                    normals[index] = Vector3.up;

                    vertices[index + 1] = new Vector3((x + 1) * tileSize, map[x, y].Elevation * elevationStep, y * tileSize);
                    uv[index + 1] = new Vector2(((float)x + 1) / map.Width, (float)y / map.Length);
                    normals[index + 1] = Vector3.up;

                    vertices[index + 2] = new Vector3(x * tileSize, map[x, y].Elevation * elevationStep, (y + 1) * tileSize);
                    uv[index + 2] = new Vector2((float)x / map.Width, ((float)y + 1) / map.Length);
                    normals[index + 2] = Vector3.up;

                    vertices[index + 3] = new Vector3((x + 1) * tileSize, map[x, y].Elevation * elevationStep, (y + 1) * tileSize);
                    uv[index + 3] = new Vector2(((float)x + 1) / map.Width, ((float)y + 1) / map.Length);
                    normals[index + 3] = Vector3.up;
                }
            }
            for (y = 0; y < map.Length; y++)
            {
                for (x = 0; x < map.Width; x++)
                {
                    GenerateMeshTileTriangles(triangles, (y * map.Width + x) * 6, x, y, map.Width);
                }
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.subMeshCount = 2;
            mesh.normals = normals;
            return mesh;
        }


        public static void GenerateMeshTileTriangles(int[] triangles, int index, int x, int y, int stride)
        {
            triangles[index] = (y * stride + x) * 4;
            triangles[index + 1] = ((y * stride + x) * 4) + 2;
            triangles[index + 2] = ((y * stride + x) * 4) + 1;
            triangles[index + 3] = ((y * stride + x) * 4) + 2;
            triangles[index + 4] = ((y * stride + x) * 4) + 3;
            triangles[index + 5] = ((y * stride + x) * 4) + 1;
        }
        public static void GenerateMeshTileTriangles(ref NativeArray<int> triangles, int index, int x, int y, int stride)
        {
            triangles[index] = (y * stride + x) * 4;
            triangles[index + 1] = ((y * stride + x) * 4) + 2;
            triangles[index + 2] = ((y * stride + x) * 4) + 1;
            triangles[index + 3] = ((y * stride + x) * 4) + 2;
            triangles[index + 4] = ((y * stride + x) * 4) + 3;
            triangles[index + 5] = ((y * stride + x) * 4) + 1;
        }

        public static void GenerateMeshTileTriangles(int[] triangles, int index, int stride, params Point[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                GenerateMeshTileTriangles(triangles, index + (i * 6), points[i].x, points[i].y, stride);
            }
        }
        public static void GenerateMeshTileTriangles(int[] triangles, int index, int stride, IEnumerator<Point> points)
        {
            for (int i = 0; points.MoveNext(); i++)
            {
                GenerateMeshTileTriangles(triangles, index + (i * 6), points.Current.x, points.Current.y, stride);
            }

        }
        public static void GenerateMeshTileTriangles(ref NativeArray<int> triangles, int index, int stride, IEnumerator<Point> points)
        {
            for (int i = 0; points.MoveNext(); i++)
            {
                GenerateMeshTileTriangles(ref triangles, index + (i * 6), points.Current.x, points.Current.y, stride);
            }

        }




    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Point : IEquatable<Point>
    {
        public static readonly Point zero = new Point(0, 0);

        public ushort x;
        public ushort y;

        public Point(ushort x, ushort y)
        {
            this.x = x;
            this.y = y;
        }
        public Point(int x, int y)
        {
            this.x = Convert.ToUInt16(x);
            this.y = Convert.ToUInt16(y);
        }

        public bool ComparePoints(Point comparePoint)
        {
            if (this.x == comparePoint.x && this.y == comparePoint.y)
                return true;
            return false;
        }

        public bool InRange(Point comparePoint, ushort range)
        {
            if (this.Distance(comparePoint) <= range)
                return true;
            else
                return false;
            /*if ((this.x + this.y) <= (comparePoint.x + comparePoint.y + range) &&
            (this.x + this.y) >= (comparePoint.x + comparePoint.y - range) &&
            (this.x <= comparePoint.x + range) &&
            (this.x >= comparePoint.x - range) &&
            (this.y <= comparePoint.y + range) &&
            (this.y >= comparePoint.y - range))
                return true;
            return false;*/
        }
        
        /// <summary>
        /// Fill the NativeList given with all the end points in the range
        /// </summary>
        public void GetEndpoints(ref NativeList<Point> points, int range, ushort maxLength, ushort maxWidth)
        {
            //maybe useless
        }

        public bool IsEndpoint(Point targetPoint, int range, ushort maxLength, ushort maxWidth)
        {
            if (!this.ComparePoints(targetPoint))
            {
                if (this.Distance(targetPoint) == range)
                    return true;
                if (targetPoint.x == 0 || targetPoint.x == maxLength - 1 || targetPoint.y == 0 || targetPoint.y == maxWidth - 1)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// used to do line of sight stuff. doesn't work yet oops
        /// </summary>
        public void CalculateLOS(ref NativeList<Point> fullPathToTargetPoint, Point targetPoint, int range, ushort maxLength, ushort maxWidth)
        {
            //Returns a list of points to sort of simulate line of sight from one point to another.
            //int index = points.Length;
            //points.Add(this);
            //index++;
            
            //idk why this would happen but it could I guess
            if (this.ComparePoints(targetPoint))
                return;

            //points.Add(targetPoint);
            //index++;

            /*//Again, not sure why this would happen but...
            if (this.Distance(targetPoint) == 1) //oh. that uh. yeah. wow. I'm dumb???
                return;*/

            #region bleh
            /*
            if (this.x == targetPoint.x)
            {
                //then we just need to add every point from the origin, to the uhhh. Y. thing.
                int deltaY = math.abs(targetPoint.y - this.y);
                int sign = 1;
                if (targetPoint.y < this.y)
                    sign = -1;
                for (int i = deltaY; i <= range; i++)
                {
                    int newY = this.y + (ushort)(sign * i);

                    if (newY >= 0)
                        points.Add(new Point(this.x, newY));
                }
            }
            else if (this.y == targetPoint.y)
            {
                int deltaX = math.abs(targetPoint.x - this.x);
                int sign = 1;
                if (targetPoint.x < this.x)
                    sign = -1;
                for (int i = deltaX; i <= range; i++)
                {
                    int newX = this.x + (ushort)(sign * i);

                    if (newX >= 0)
                        points.Add(new Point(newX, this.y));
                }
            }*/
            #endregion

            if (this.x == targetPoint.x)//11 and 12
            {
                //just add every point between them...
                if (targetPoint.y > this.y) //2 > 1
                {
                    for (int y = this.y; y <= targetPoint.y; y++)
                    {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
                else
                {
                    for (int y = this.y; y >= targetPoint.y; y--)
                    {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
            }
            else if (this.y == targetPoint.y)
            {
                //just add every point between them...
                if (targetPoint.x > this.x)
                {
                    for (int x = this.x; x <= targetPoint.x; x++)
                    {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
                else
                {
                    for (int x = this.x; x >= targetPoint.x; x--)
                    {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
            }
            else
            {
                //bresenham line, look it up
                //11 and 22, range 3
                //so we should have x = 2, x < 4(?) x++
                int x0 = this.x;
                int y0 = this.y;
                int x1 = targetPoint.x;
                int y1 = targetPoint.y;
                bool steep = math.abs(targetPoint.y - this.y) > math.abs(targetPoint.x - this.x);

                if (steep)
                {
                    var temp = x0;
                    x0 = y0;
                    y0 = temp;

                    temp = x1;
                    x1 = y1;
                    y1 = temp;
                }
                if (x0 > x1)
                {
                    //Note that this saves the list from target -> origin, instead of origin -> target
                    var temp = x0;
                    x0 = x1;
                    x1 = temp;

                    temp = y0;
                    y0 = y1;
                    y1 = temp;
                }

                int deltaX = x1 - x0; //1
                int deltaY = Math.Abs(y1 - y0); //1
                int error = 0;
                int ystep;
                int y = y0; //1
                //int lastCheckXPoint = range - x1;
                //need a number other than 10 here...
                //x has to be <= origin.x + range - abs(range - deltaX)
                //1 + 3 
                if (y0 < y1) ystep = 1; else ystep = -1;//1
                for (int x = x0; x <= x1; x++) 
                {
                    if (steep)
                        fullPathToTargetPoint.Add(new Point(y, x));
                    else
                        fullPathToTargetPoint.Add(new Point(x, y));

                    error += deltaY;
                    
                    if (2 * error >= deltaX) 
                    {
                        y += ystep;
                        error -= deltaX;
                    }
                }

                #region bleh
                /*
                for (int x = x1; x <= 10; x++) //x=2, x<
                {
                    //weird break that I can fix later
                    if (x > maxLength || x < 0 || y > maxWidth || y < 0)
                        break;
                    if (steep) 
                        result.Add(new Point(y, x));
                    else 
                        result.Add(new Point(x, y));

                    error += deltaY;
                    if (2 * error >= deltaX) 
                    {
                        y += ystep;
                        error -= deltaX;
                    }
                }
                */
                #endregion
            }
        }

        
        public void CalculateLOS2(ref NativeList<Point> fullPathToTargetPoint, ref NativeList<Point> obstructions, ref DynamicBuffer<HighlightTile> tilesToHighlight, MapLayer layer,
                                    Point targetPoint, int range, ushort maxLength, ushort maxWidth)
        {
            //inclusive(?)
            //remember, the point of this is to take the list and *shave* it. which means this needs ahhh. the inaccessible tiles and stuff.
            //TODO: Make this less dumb as fuck. (how to properly manipulate a native array list...?)
            //think. is it easier to keep track of the ones to remove...? no probably not...

            //so this one gets all the points that aren't obstructed and returns them as a list. easy enough.
            if (this.x != fullPathToTargetPoint[0].x)
            {
                for (var i = fullPathToTargetPoint.Length - 1; i >= 0; i--)
                {
                    //first, find the index of the obstruction.
                    for (var j = 0; j < obstructions.Length; j++)
                    {
                        if (fullPathToTargetPoint[i].ComparePoints(obstructions[j]))
                        {
                            return;
                        }
                    }
                    //theres literally no way this works?
                    tilesToHighlight.Add((new HighlightTile{ point = fullPathToTargetPoint[i], layer = layer}));
                }
            }
            else
            {
                for (var i = 0; i < fullPathToTargetPoint.Length; i++)
                {
                    //first, find the index of the obstruction.
                    for (var j = 0; j < obstructions.Length; j++)
                    {
                        if (fullPathToTargetPoint[i].ComparePoints(obstructions[j]))
                        {
                            return;
                        }
                    }
                    //theres literally no way this works?
                    tilesToHighlight.Add((new HighlightTile{ point = fullPathToTargetPoint[i], layer = layer}));
                }
            }
        }

        public void CalculateLOS3(ref NativeList<Point> fullPathToTargetPoint, ref NativeList<Point> obstructions, ref NativeList<Point> tilesToRemove, MapLayer layer,
                                    Point targetPoint, int range, ushort maxLength, ushort maxWidth, int startingIndex)
        {
            //we gotta do this so that it calculates on a per-point basis... otherwise we're kinda fucked. which means... yeah.
            //this is gonna eat up some power!!! ahhhhh!!!!
            //exclusive(?)
            //remember, the point of this is to take the list and *shave* it. which means this needs ahhh. the inaccessible tiles and stuff.
            //TODO: Make this less dumb as fuck. (how to properly manipulate a native array list...?)
            //think. is it easier to keep track of the ones to remove...? no probably not...

            //so this one gets all the points that aren't obstructed and returns them as a list. easy enough.
            if (fullPathToTargetPoint.Length > 0)
            {
                bool found = false;
                if (this.x != fullPathToTargetPoint[startingIndex].x)
                {
                    for (var i = fullPathToTargetPoint.Length - 1; i >= startingIndex; i--)
                    {
                        //first, find the index of the obstruction.
                        if (!found)
                        {
                            for (var j = 0; j < obstructions.Length; j++)
                            {
                                if (fullPathToTargetPoint[i].ComparePoints(obstructions[j]))
                                {
                                    found = true;
                                    tilesToRemove.Add(targetPoint);
                                    return;
                                }
                            }
                        }
                        //theres literally no way this works?
                        else
                            tilesToRemove.Add(fullPathToTargetPoint[i]);
                    }
                }
                else
                {
                    for (var i = startingIndex; i < fullPathToTargetPoint.Length; i++)
                    {
                        if (!found)
                        {
                            //first, find the index of the obstruction.
                            for (var j = 0; j < obstructions.Length; j++)
                            {
                                if (fullPathToTargetPoint[i].ComparePoints(obstructions[j]))
                                {
                                    found = true;
                                    tilesToRemove.Add(targetPoint);
                                    return;
                                }
                            }
                        }
                        //theres literally no way this works?
                        else
                            tilesToRemove.Add(fullPathToTargetPoint[i]);
                    }
                }
            }
        }
        
        public void CalculateLOSXW(ref NativeList<Point> fullPathToTargetPoint, Point targetPoint, int range, ushort maxLength, ushort maxWidth, double leniency)
        {
            //idk why this would happen but it could I guess
            if (this.ComparePoints(targetPoint))
                return;

            if (this.x == targetPoint.x)
            {
                if (targetPoint.y > this.y) //2 > 1
                {
                    for (int y = this.y; y <= targetPoint.y; y++)
                    {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
                else
                {
                    for (int y = this.y; y >= targetPoint.y; y--)
                    {
                        fullPathToTargetPoint.Add(new Point(this.x, y));
                    }
                }
            }
            else if (this.y == targetPoint.y)
            {
                if (targetPoint.x > this.x)
                {
                    for (int x = this.x; x <= targetPoint.x; x++)
                    {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
                else
                {
                    for (int x = this.x; x >= targetPoint.x; x--)
                    {
                        fullPathToTargetPoint.Add(new Point(x, this.y));
                    }
                }
            }
            else
            {
                int x0 = this.x;
                int y0 = this.y;
                int x1 = targetPoint.x;
                int y1 = targetPoint.y;
                bool steep = math.abs(targetPoint.y - this.y) > math.abs(targetPoint.x - this.x);

                if (steep)
                {
                    var temp = x0;
                    x0 = y0;
                    y0 = temp;

                    temp = x1;
                    x1 = y1;
                    y1 = temp;
                }
                if (x0 > x1)
                {
                    //Note that this saves the list from target -> origin, instead of origin -> target
                    var temp = x0;
                    x0 = x1;
                    x1 = temp;

                    temp = y0;
                    y0 = y1;
                    y1 = temp;
                }

                //Don't trust this at all. Debug it. gradient probably being converted to some int or something.
                double dx = x1 - x0;
                double dy = y1 - y0;
                double gradient = dy / dx;
                if (dx == 0)
                    gradient = 1.0;

                //First endpoint
                var xEnd = math.floor(x0 + 0.5);
                var yEnd = y0 + gradient * (xEnd - x0);
                var xGap = 1 - ((x0 + 0.5) - math.floor(x0 + 0.5)); 

                var xpxl1 = xEnd;
                var ypxl1 = math.floor(yEnd);
                var firstEndPoint = new Point(0,0);

                if (steep)
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        firstEndPoint = new Point((int)ypxl1, (int)xpxl1);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        firstEndPoint = new Point((int)ypxl1 + 1, (int)xpxl1);
                }
                else
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        firstEndPoint = new Point((int)xpxl1, (int)ypxl1);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        firstEndPoint = new Point((int)xpxl1, (int)ypxl1 + 1);
                }
                var interY = yEnd + gradient;

                //Second endpoint
                xEnd = math.floor(x1 + 0.5);
                yEnd = y1 + gradient * (xEnd - x1);
                xGap = 1 - ((x1 + 0.5) - math.floor(x1 + 0.5));

                var xpxl2 = xEnd;
                var ypxl2 = math.floor(yEnd);
                Point secondEndPoint = new Point (0,0);

                if (steep)
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        secondEndPoint = new Point((int)ypxl2, (int)xpxl2);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        secondEndPoint = new Point((int)ypxl2 + 1, (int)xpxl2);
                }
                else
                {
                    var thing = (1 - ((yEnd) - math.floor(yEnd))) * xGap;
                    if (thing > leniency)
                        secondEndPoint = new Point((int)xpxl2, (int)ypxl2);
                    var thing2 = (yEnd - math.floor(yEnd)) * xGap;
                    if (thing2 > leniency)
                        secondEndPoint = new Point((int)xpxl2, (int)ypxl2 + 1);
                }
                
                //not sure if right
                fullPathToTargetPoint.Add(firstEndPoint);

                //main loop
                if (steep)
                {
                    for (int x = (int)xpxl1 + 1; x < (int)xpxl2; x++)
                    {
                        var thing = 1 - ((interY) - math.floor(interY));
                        if (thing > leniency)
                            fullPathToTargetPoint.Add(new Point((ushort)math.floor(interY), x));
                        var thing2 = interY - math.floor(interY);
                        if (thing2 > leniency)
                            fullPathToTargetPoint.Add(new Point((ushort)math.floor(interY) + 1, x));
                        interY += gradient;
                    }
                }
                else
                {
                    for (int x = (int)xpxl1 + 1; x < (int)xpxl2; x++)
                    {
                        var thing = 1 - ((interY) - math.floor(interY));
                        if (thing > leniency)
                            fullPathToTargetPoint.Add(new Point(x, (ushort)math.floor(interY)));
                        var thing2 = interY - math.floor(interY);
                        if (thing2 > leniency)
                            fullPathToTargetPoint.Add(new Point(x, (ushort)math.floor(interY) + 1));
                        interY += gradient;
                    }
                }

                fullPathToTargetPoint.Add(secondEndPoint);
            }
        }
        /*
        1 - ((x) - math.abs(x)) is rfpart
        (x) - math.floor(x) is fpart
        math.floor(x) is ipart
        math.floor(x + 0.5) is round
        function plot(x, y, c) is
    plot the pixel at (x, y) with brightness c (where 0 ≤ c ≤ 1)

// integer part of x
function ipart(x) is
    return floor(x)

function round(x) is
    return ipart(x + 0.5)

// fractional part of x
function fpart(x) is
    return x - floor(x)

function rfpart(x) is
    return 1 - fpart(x)

function drawLine(x0,y0,x1,y1) is
    boolean steep := abs(y1 - y0) > abs(x1 - x0)
    
    if steep then
        swap(x0, y0)
        swap(x1, y1)
    end if
    if x0 > x1 then
        swap(x0, x1)
        swap(y0, y1)
    end if
    
    dx := x1 - x0
    dy := y1 - y0
    gradient := dy / dx
    if dx == 0.0 then
        gradient := 1.0
    end if

    // handle first endpoint
    xend := round(x0)
    yend := y0 + gradient * (xend - x0)
    xgap := rfpart(x0 + 0.5)
    xpxl1 := xend // this will be used in the main loop
    ypxl1 := ipart(yend)
    if steep then
        plot(ypxl1,   xpxl1, rfpart(yend) * xgap)
        plot(ypxl1+1, xpxl1,  fpart(yend) * xgap)
    else
        plot(xpxl1, ypxl1  , rfpart(yend) * xgap)
        plot(xpxl1, ypxl1+1,  fpart(yend) * xgap)
    end if
    intery := yend + gradient // first y-intersection for the main loop
    
    // handle second endpoint
    xend := round(x1)
    yend := y1 + gradient * (xend - x1)
    xgap := fpart(x1 + 0.5)
    xpxl2 := xend //this will be used in the main loop
    ypxl2 := ipart(yend)
    if steep then
        plot(ypxl2  , xpxl2, rfpart(yend) * xgap)
        plot(ypxl2+1, xpxl2,  fpart(yend) * xgap)
    else
        plot(xpxl2, ypxl2,  rfpart(yend) * xgap)
        plot(xpxl2, ypxl2+1, fpart(yend) * xgap)
    end if
    
    // main loop
    if steep then
        for x from xpxl1 + 1 to xpxl2 - 1 do
           begin
                plot(ipart(intery)  , x, rfpart(intery))
                plot(ipart(intery)+1, x,  fpart(intery))
                intery := intery + gradient
           end
    else
        for x from xpxl1 + 1 to xpxl2 - 1 do
           begin
                plot(x, ipart(intery),  rfpart(intery))
                plot(x, ipart(intery)+1, fpart(intery))
                intery := intery + gradient
           end
    end if
end function
        */
        public Point ShiftX(int amount)
        {
            return new Point(x + amount, y);
        }
        public bool ShiftX(int amount, int max, out Point output)
        {
            int newX = x + amount;
            if (newX < 0 || newX >= max)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(newX, y);
                return true;
            }
        }
        public Point ShiftY(int amount)
        {
            return new Point(x, y + amount);
        }
        public bool ShiftY(int amount, int max, out Point output)
        {
            int newY = y + amount;
            if (newY < 0 || newY >= max)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(x, newY);
                return true;
            }
        }

        public Point Shift(int xAmount, int yAmount)
        {
            return new Point(x + xAmount, y + yAmount);
        }

        public bool Shift(int xAmount, int yAmount, int xMax, int yMax, out Point output)
        {
            int newY = y + yAmount;
            int newX = x + xAmount;
            if (newY < 0 || newX < 0 || newX >= xMax || newY >= yMax)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(newX, newY);
                return true;
            }
        }

        public uint Distance(Point other)
        {
            return (uint)(abs(x - other.x) + abs(y - other.y));
        }

        public static bool Create(int x, int y, int maxX, int maxY, out Point output)
        {
            if (x < 0 || x >= maxX || y < 0 || y >= maxY)
            {
                output = default;
                return false;
            }
            else
            {
                output = new Point(x, y);
                return true;
            }
        }


        public bool Equals(Point other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is Point point && Equals(point);
        }

        public override int GetHashCode()
        {
            int hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return "Point(" + x + "," + y + ")";
        }

        public static Point operator +(Point thisPoint, Point otherPoint) => new Point(thisPoint.x + otherPoint.x, thisPoint.y + otherPoint.y);

        public static Point operator -(Point thisPoint, Point otherPoint) => new Point(thisPoint.x - otherPoint.x, thisPoint.y - otherPoint.y);
        public static implicit operator Vector2Int(Point point) => new Vector2Int(Convert.ToInt32(point.x), Convert.ToInt32(point.y));
        public static explicit operator Point(Vector2Int vector)
        {
            if (vector.x >= 0 && vector.y >= 0)
                if (vector.x == 0 && vector.y == 0)
                    return zero;
                else
                    return new Point((ushort)vector.x, (ushort)vector.y);
            else
                throw new InvalidCastException("Vector must be positive");
        }

    }


    public class PointComparerByX : IComparer<Point>
    {
        private static PointComparerByX INSTANCE;

        public static PointComparerByX GetInstance()
        {
            if (INSTANCE == null)
                INSTANCE = new PointComparerByX();
            return INSTANCE;
        }
        public int Compare(Point a, Point b)
        {
            int yCompare = a.y.CompareTo(b.y);
            return yCompare != 0 ? yCompare : a.x.CompareTo(b.x);
        }

        private PointComparerByX() { }
    }

    public class PointComparerByY : IComparer<Point>
    {
        private static PointComparerByY INSTANCE;

        public static PointComparerByY GetInstance()
        {
            if (INSTANCE == null)
                INSTANCE = new PointComparerByY();
            return INSTANCE;
        }
        public int Compare(Point a, Point b)
        {
            int xCompare = a.x.CompareTo(b.x);
            return xCompare != 0 ? xCompare : a.y.CompareTo(b.y);
        }
        private PointComparerByY() { }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct SpawnGroup : IMapSpawnGroup
    {
        [SerializeField]
        public Point[] points;

        public Point this[int index] => points[index];

        public int Count => points.Length;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Tile : IMapTile
    {
        public int elevation;


        public bool inaccessible;

        public int Elevation => elevation;

        public bool Inaccessible => inaccessible;
    }

    public struct BlittableTile : IMapTile
    {

        public int elevation;

        public BlittableBool inaccessible;
        public int Elevation { get => elevation; }

        public bool Inaccessible { get => inaccessible; }

        public static implicit operator Tile(BlittableTile tile) => new Tile
        {
            elevation = tile.elevation,
            inaccessible = tile.inaccessible
        };


        public static explicit operator BlittableTile(Tile tile) => new BlittableTile
        {
            elevation = tile.elevation,
            inaccessible = tile.inaccessible
        };

    }
    public struct TileInfo : IEquatable<TileInfo>
    {
        public readonly Point point;
        public readonly Tile tile;
        public TileInfo(Point point, Tile tile)
        {
            this.point = point;
            this.tile = tile;
        }

        bool IEquatable<TileInfo>.Equals(TileInfo other)
        {
            return other.point.Equals(point) && other.tile.Equals(tile);
        }
    }

    public enum MapLayer
    {
        BASE,
        PLAYER_MOVE,
        PLAYER_ATTACK,
        PLAYER_SUPPORT,
        PLAYER_ALL,
        ENEMY_MOVE,
        ENEMY_ATTACK,
        ENEMY_SUPPORT,
        ENEMY_ALL,
        ALLY_MOVE,
        ALLY_ATTACK,
        ALLY_SUPPORT,
        ALLY_ALL,
        UTILITY,
        HOVER
    }


}