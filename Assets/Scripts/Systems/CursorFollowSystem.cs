using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
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
    //https://docs.unity3d.com/Packages/com.unity.entities@0.0/manual/component_group.html may be helpful if grid = ecs (jk it's dynamic buffer or smth)
    //this needs to know the current control scheme for sure... otherwise it doesn't make sense.
    //it would either A: do raycasting bullshit or B: copy it to the camera data here...
    //so for experiment purposes we should get that going *now* rather than later... which means we probably have to figure out thecontrols cheme stuff now
    
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
/*
if (pointerPresent)
        {
            Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(currentMousePosition);
            if (mapRenderer.GetComponent<MeshCollider>().Raycast(ray, out RaycastHit hitInfo, rayDistance))
            {
                transform.position = ray.GetPoint(hitInfo.distance);
            }
        }
*/