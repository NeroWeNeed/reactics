using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Reactics.Battle;

[UpdateInGroup(typeof(Unity.Entities.SimulationSystemGroup))]
[UpdateAfter(typeof(PlayerInputSystem))]
public class CameraMovementSystem : JobComponentSystem
{
    private float screenEdgeLength = 40f;

    //public ComponentDataFromEntity<Translation> translationData;

    /*protected override void OnCreate()// for some reason this doesn't work at all. probably has an explanation.
    {
        Entities.ForEach((ref Translation trans, ref CameraMovementData data) =>
            {
                data.cameraLookAtPoint = new float3(0, 0, 0); //thsi is the origin, later it will be calculated or w/e.
                trans.Value = math.normalize(trans.Value) * data.offsetValue;
                data.zoomMagnitude = 1f;
                data.lowerZoomLimit = 0.1f;
                data.upperZoomLimit = 2.0f;
            }).Run();
    }*/
    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation trans, ref CameraMovementData moveData, in Rotation rot, in CameraRotationData rotData, in ControlSchemeData controlSchemeData) => 
        {
            if (!rotData.rotating) //delete this if you want to pan/zoom and rotate at the same time.
            {
            bool moving = false;
            float screenEdgeLength = 40f;
            //TODO: Vector math stuff (confirm whether the magnitude is the same for any given rotation... it probably isn't
            //TODO: Have an actual variable for ScreenEdgeLength instead of passing in stuff.
            float3 upwardDirection = math.mul(rot.Value, new float3(0, 0, moveData.speed * deltaTime));
            upwardDirection.y = 0;

            if (moveData.movementDirection.y > 0.1f)
            {
                trans.Value += upwardDirection;
                moveData.cameraLookAtPoint += upwardDirection;
                moving = true;
            }
            if (moveData.movementDirection.x > 0.1f)
            {
                float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                trans.Value += val;
                moveData.cameraLookAtPoint += val;
                moving = true;
            }
            if (moveData.movementDirection.x < -0.1f)
            {
                float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                trans.Value -= val;
                moveData.cameraLookAtPoint -= val;
                moving = true;
            }
            if (moveData.movementDirection.y < -0.1f)
            {
                trans.Value -= upwardDirection;
                moveData.cameraLookAtPoint -= upwardDirection;
                moving = true;
            }

            //zoom goes here since it just uses the same components anyway...?
            if (moveData.zoomDirectionAndStrength > 100f)
            {
                moveData.zoomMagnitude -= 0.1f;
            }
            else if (moveData.zoomDirectionAndStrength > 0.5f)
            {
                moveData.zoomMagnitude -= 0.1f;
            }
            else if (moveData.zoomDirectionAndStrength < -0.5f)
            {
                moveData.zoomMagnitude += 0.1f;
            }
            else if (moveData.zoomDirectionAndStrength < -100f)
            {
                moveData.zoomMagnitude += 0.1f;
            }
            
            moveData.zoomMagnitude = Mathf.Clamp(moveData.zoomMagnitude, moveData.lowerZoomLimit, moveData.upperZoomLimit);
            trans.Value = math.normalize(trans.Value - moveData.cameraLookAtPoint) * (moveData.offsetValue * moveData.zoomMagnitude) + moveData.cameraLookAtPoint;

            if (!moving && controlSchemeData.currentControlScheme == ControlSchemes.Gamepad) //make this an enum and use it somehow/
            {
                //snap to nearest tile here... we don't have that data atm, though, so we'll have to do testing instead... sigh.
                //this code would be getting reached, however, currently gamepad movement never stops... so yeah. it technically doesn't happen atm.
                //trans.Value.y += 1.0f;
            }
            }
        }).Run();
        return default;
    }
}