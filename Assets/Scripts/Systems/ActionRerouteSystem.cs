using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Collections;
using System;
using Unity.Burst;
using Reactics.Commons;
public class ActionRerouteSystem : SystemBase
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
            return 0;
        }

        public bool Equals(Thing other)
        {
            return false;
        }
    }

    EntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate() 
    {
        //write a system that applies only to currently moving map bodies.
        //It will find the targeted map body (do map bodies need a target now...?) and see if its in range of its projectile.
        //if it's not, then we gotta do some stuff. (some stuff being uhhh. get closer.) 
        //we can cheese it to test it by like... flubbing one of the checks and then passing the right effect later. 
        //so when it starts to move it'll realize they're out of range and fix itself.
        Entities.ForEach((int entityInQueryIndex, Entity entity, ref Projectile proj, ref MapBody mapBody, ref MapBodyTranslation trans) => 
        {
            /*//hi how about we do this for projectile highlight tile range instead of here?
            //then we can just kinda reuse it, right?
            //First just see if this works. if it does then, well, we've got something going.
            //It does work! And it somehow... stays in range?? That's great! Now we just need to uhhh. Have it go... good. places.
            trans.point = new Point (5,5);
            //So this should run on map bodies. with projectiles. because they can have those now, yeah?
            //Ah, but the issue was: how do we update the uhhhh. thing exactly? y'know. the mapbody step or w/e.
            //There's probably a point where it's sitting still (either mapbodytranslation or mapbodytranslationstep.)
            //That's where we'd want to recalculate and set a new goalpost, right?

            //First, see if they're in range. if they are, then do nothing.
            if (trans.point.Distance(proj.targetPoint) < proj.effect.range) //for now just do a fuKCLJFNIBBBBBBBwf tile
                return;
            //Ah yes, the conundrum: how do we get the point if we can't query the map body?
            //Well, we just have to query the translation and do it that way, right?
            //Either that, or we need a system to update the projectile's target tile every single damn time they move which seems... questionable at best...
            //If they aren't in range, then check if they're in range assuming we move the maximum. 
            //highlight tiles
            NativeList<Thing> history = new NativeList<Thing>(Allocator.Temp);
            NativeHeap<Thing> frontier = new NativeHeap<Thing>(Allocator.Temp);
            Thing origin = new Thing(mapBody.point, 0);
            frontier.Add(origin);
            history.Add(origin);
            Thing current;
            //Thing closest = frontier.Peek();
            NativeArray<Thing> expansion = new NativeArray<Thing>(4, Allocator.Temp);
            while (frontier.Pop(out current))
            {
                history.Add(current);
                //check all four directions
                //we don't need the best path for highlighting we just need to know a path exists. so we can do the neighbors thing. and make this easy.
                //if ()
            }
            //If they are, then recalculate our destination to the LOWEST COST IN RANGE TILE.
            */
        }).Schedule();
    }
}
/*
more cheating but good this time'
// Funtion that implements Dijkstra's 
// single source shortest path 
// algorithm for a graph represented 
// using adjacency matrix representation 
void dijkstra(int graph[V][V], int src) 
{ 
      
    // The output array. dist[i] 
    // will hold the shortest 
    // distance from src to i 
    int dist[V];  
  
    // sptSet[i] will true if vertex 
    // i is included / in shortest 
    // path tree or shortest distance  
    // from src to i is finalized 
    bool sptSet[V]; 
  
    // Parent array to store 
    // shortest path tree 
    int parent[V]; 
  
    // Initialize all distances as  
    // INFINITE and stpSet[] as false 
    for (int i = 0; i < V; i++) 
    { 
        parent[0] = -1; 
        dist[i] = INT_MAX; 
        sptSet[i] = false; 
    } 
  
    // Distance of source vertex  
    // from itself is always 0 
    dist[src] = 0; 
  
    // Find shortest path 
    // for all vertices 
    for (int count = 0; count < V - 1; count++) 
    { 
        // Pick the minimum distance 
        // vertex from the set of 
        // vertices not yet processed.  
        // u is always equal to src 
        // in first iteration. 
        int u = minDistance(dist, sptSet); 
  
        // Mark the picked vertex  
        // as processed 
        sptSet[u] = true; 
  
        // Update dist value of the  
        // adjacent vertices of the 
        // picked vertex. 
        for (int v = 0; v < V; v++) 
  
            // Update dist[v] only if is 
            // not in sptSet, there is 
            // an edge from u to v, and  
            // total weight of path from 
            // src to v through u is smaller 
            // than current value of 
            // dist[v] 
            if (!sptSet[v] && graph[u][v] && 
                dist[u] + graph[u][v] < dist[v]) 
            { 
                parent[v] = u; 
                dist[v] = dist[u] + graph[u][v]; 
            }  
    } 
  
    // print the constructed 
    // distance array 
    printSolution(dist, V, parent); 
} 
cheating
S indicates the start, E indicates the end
def find_path_bfs(s, e, grid):
    queue = [(s, [])]  # start point, empty path

    while len(queue) > 0:
        node, path = queue.pop(0)
        path.append(node)
        mark_visited(node, v)

        if node == e:
            return path

        adj_nodes = get_neighbors(node, grid)
        for item in adj_nodes:
            if not is_visited(item, v):
                queue.append((item, path[:]))

    return None  # no path found

    cheating more
    def find_path_bfs(s, e, grid):
    queue = list()
    path = list()
    queue.append(s)

    while len(queue) > 0:
        node = queue.pop(0)
        path.append(node)
        mark_visited(node, v)

        if node == e:
            break

        adj_nodes = get_neighbors(node, grid)
        for item in adj_nodes:
            if is_visited(item, v) is False:
                queue.append(item)

    return path
*/
/*
the process:
say we're moving somewhere and they mvoe out of range.
well, each time they change tiles, we need to do this.
using the range of our action, draw a grid around *them.*
get any tiles in this grid in our movement range. from there, we do a cost search.

where do we do the cost search from, though?
We should do it from... well. oof.

there are a lot of things to consider. we should probably pick one of them.
let's just do it from MOVETILE, since that doesn't move and makes things a lot easier.
if we did it from current position we'd have to recalculate every time they move and every time we move... probably. which would be like.. ew.
there are certainly drawbacks to both, but at least with movetile there's the added benefit of "we aren't moving."
ah, but what about teleports, then...?
we do have to do it from us because that's a more accurate representation of our movement. but don't we have to take the move tile into account or it's dumb...?
eh. just do it from movetile. we can take teleports into account later or something. just keep it in the back of your mind or w/e...
*/