using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[AlwaysSynchronizeSystem]
[UpdateAfter(typeof(PlayerInputSystem))]
public class CameraMovementSystem : JobComponentSystem
{
    private float screenEdgeLength = 40f;

    //public ComponentDataFromEntity<Translation> translationData;

    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {
        Entities.ForEach((ref Translation trans, ref CameraMovementData data, in Rotation rot) => 
        {
            float screenEdgeLength = 40f;
            //TODO: Vector math stuff (confirm whether the magnitude is the same for any given rotation... it probably isn't
            //TODO: Have an actual variable for ScreenEdgeLength instead of passing in stuff.
            float3 upwardDirection = math.mul(rot.Value, new float3(0, 0, data.speed));
            upwardDirection.y = 0;

            if (data.movementDirection.y > 0.1f)
            {
                trans.Value += upwardDirection;
                data.cameraLookAtPoint += upwardDirection;
            }
            if (data.movementDirection.x > 0.1f)
            {
                float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                trans.Value += val;
                data.cameraLookAtPoint += val;
            }
            if (data.movementDirection.x < -0.1f)
            {
                float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                trans.Value -= val;
                data.cameraLookAtPoint -= val;
            }
            if (data.movementDirection.y < -0.1f)
            {
                trans.Value -= upwardDirection;
                data.cameraLookAtPoint -= upwardDirection;
            }

            //zoom goes here since it just uses the same components anyway...?
            if (data.zoomDirectionAndStrength > 100f)
            {
                data.zoomMagnitude -= 0.01f;
            }
            else if (data.zoomDirectionAndStrength > 0.5f)
            {
                data.zoomMagnitude -= 0.01f;
            }
            else if (data.zoomDirectionAndStrength < -0.5f)
            {
                data.zoomMagnitude += 0.01f;
            }
            else if (data.zoomDirectionAndStrength < -100f)
            {
                data.zoomMagnitude += 0.01f;
            }
            
            data.zoomMagnitude = Mathf.Clamp(data.zoomMagnitude, data.lowerZoomLimit, data.upperZoomLimit);

            //this *definitely* doesn't work
            //we need to approach this entirely differently probably, it seems
            //Having a "lookatpoint" is great and all and will probably be important for like wait.
            //trans.Value = (trans.Value - data.cameraLookAtPoint) * data.zoomMagnitude;
        }).Run();
        return default;
    }
}