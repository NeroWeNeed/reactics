using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Reactics.Battle;
using Unity.Physics;

//always synchronize? not sure if necessary on component system
[UpdateInGroup(typeof(Unity.Entities.SimulationSystemGroup))]
[UpdateAfter(typeof(CameraRotationSystem))]
public class CursorFollowSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {
        ComponentDataFromEntity<CameraMovementData> cameraData = GetComponentDataFromEntity<CameraMovementData>(true);
        //Apparently doing this is just allowed.
        var physicsWorldSystem = this.World.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        Entities.ForEach((ref Translation trans, in CursorData cursorData, in ControlSchemeData controlSchemeData) =>
        {
            if (!cameraData.Exists(cursorData.cameraEntity))
                return;
            if (controlSchemeData.currentControlScheme == ControlSchemes.Gamepad)
            {
                trans.Value = cameraData[cursorData.cameraEntity].cameraLookAtPoint;
            }
            else if (controlSchemeData.currentControlScheme == ControlSchemes.KeyboardAndMouse)
            {
                //Since we have some ray stuff from the camera we don't need a Ray object
                RaycastInput input = new RaycastInput
                {
                    Start = cursorData.rayOrigin, //Start point
                    End = cursorData.rayOrigin + (cursorData.rayDirection * cursorData.rayMagnitude), //End point
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u, //allow everything to collide with this ray
                        CollidesWith = ~0u, //make this ray try to collide with everything
                        GroupIndex = 0 //idk that's what everyone else did
                    }
                };
                RaycastHit hit = new RaycastHit();
                bool haveHit = collisionWorld.CastRay(input, out hit);
                if (haveHit)
                {
                    trans.Value = hit.Position;
                }
                //UnityEngine.Debug.DrawRay(cursorData.rayOrigin, cursorData.rayDirection * cursorData.rayMagnitude, UnityEngine.Color.yellow);
            }
        }).Run();
        return default;
    }
}

//move to own cs file 
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(CursorFollowSystem))]
public class CursorHighlightSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {/* broken due to new map stuff
        ComponentDataFromEntity<CameraMovementData> cameraData = GetComponentDataFromEntity<CameraMovementData>(true);
        //tilesFromEntity = GetBufferFromEntity<MapTile>(),
        BufferFromEntity<MapTile> tilesFromEntity = GetBufferFromEntity<MapTile>(true);
        BufferFromEntity<HighlightTile> highlightTilesFromEntity = GetBufferFromEntity<HighlightTile>(false);
        ComponentDataFromEntity<MapHeader> headerFromEntity = GetComponentDataFromEntity<MapHeader>(true);
        Entities.ForEach((ref Translation trans, ref CursorData cursorData) => //remove ref trans later it doesn't need to be ref
        {
            /*float tileSize = 200f;
            MapHeader header = headerFromEntity[cursorData.map];
            if (highlightTilesFromEntity.Exists(cursorData.map))
            {
                DynamicBuffer<HighlightTile> highlightTiles = highlightTilesFromEntity[cursorData.map];
            }*//*
            if (tilesFromEntity.Exists(cursorData.map))
            {
                DynamicBuffer<MapTile> tiles = tilesFromEntity[cursorData.map];
                DynamicBuffer<HighlightTile> highlightTiles = highlightTilesFromEntity[cursorData.map];
                //MapHeader header = headerFromEntity[cursorData.map]; works
                Point pointInfo = new Point((ushort)((trans.Value.x) / tileSize), (ushort)((trans.Value.z) / tileSize)); //this is what we want...? (apparently it should be x - trans.x)
                cursorData.lastHoverPoint = pointInfo;

                //Surely there's a better way? 
                //Granted the thing can't be that big but still?
                for (int i = 0; i < highlightTiles.Length; i++)
                {
                    if (highlightTiles[i].layer == MapLayer.HOVER)
                    {
                        highlightTiles.RemoveAt(i);
                        //Realistically there should only be one hover tile...
                        break;
                    }
                }
                //Maybe add a Point constructor or clone function to make a Point using a Point
                highlightTiles.Add(new HighlightTile { point = new Point((ushort)((trans.Value.x) / tileSize), (ushort)((trans.Value.z) / tileSize)), layer = MapLayer.HOVER });
            }
        }).Run();*/
        return default;
    }
}