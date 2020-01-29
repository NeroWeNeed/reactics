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
[UpdateAfter(typeof(CameraMovementSystem))]
public class CameraRotationSystem : JobComponentSystem //change this to OrbitSystem, perhaps
{
    protected override JobHandle OnUpdate(JobHandle inputDeps) 
    {
        //float deltaTime = Time.DeltaTime; this doesn't work
        Entities.ForEach((ref Translation trans, ref Rotation rot, ref CameraRotationData rotationData, in CameraMovementData movementData) => 
        {
            if (!rotationData.rotating && rotationData.rotationDirection.magnitude > 0.1f) //if not already rotating and an input is detected
            {
                float rotationDegrees = 0f;
                if (rotationData.rotationDirection.x > 0.1f)
                {
                    rotationDegrees = -math.radians(360f/rotationData.horizontalAngles);
                    float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    dir = math.mul(quaternion.Euler(0,rotationDegrees,0), dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;
                }
                else if (rotationData.rotationDirection.x < -0.1f)
                {
                    rotationDegrees = math.radians(360f/rotationData.horizontalAngles);
                    float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    dir = math.mul(quaternion.Euler(0,rotationDegrees,0), dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;
                }
                /*(remember when subtracting vectors a-b is pointing from a to b)
                TODO: When rotating around the z axis, the rotation of the lookatpoint hasn't changed, so we just rotate around it's z, despite our current rotation.
                figure out a way to rotate that as well whenever the camera rotates maybe, so they're always facing the same direction.
                Rotating around the Y was never an issue because that's basically just always 0 for our purposes. But we may as well change that to be proper eventually.
                */
                else if (rotationData.rotationDirection.y > 0.1f)
                {
                    //float3 upwardDirection = math.mul(rot.Value, new float3(0, 0, data.speed));
                    //upwardDirection.y = 0;
                    if (rotationData.lockToHalfVerticalSphere)
                    {
                        rotationDegrees = math.radians(180f/rotationData.verticalAngles);
                    }
                    else
                    {
                        rotationDegrees = math.radians(360f/rotationData.verticalAngles);
                    }
                    //do something once it works
                    float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    quaternion killme = math.mul(quaternion.Euler(math.radians(30),0,0), rot.Value);
                    dir = math.mul(killme, dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;
                }
                else if (rotationData.rotationDirection.y < -0.1f)
                {
                    if (rotationData.lockToHalfVerticalSphere)
                    {
                        rotationDegrees = -math.radians(180f/rotationData.verticalAngles);
                    }
                    else
                    {
                        rotationDegrees = -math.radians(360f/rotationData.verticalAngles);
                    }
                    //ok yup that was my last idea it's too late for this garbage I'll be back later
                    float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    quaternion killme = math.mul(quaternion.Euler(math.radians(30),0,0), rot.Value);
                    dir = math.mul(killme, dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;
                    /*float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    quaternion rotation = quaternion.LookRotation(math.normalize(dir), Vector3.up);
                    rotation = math.mul(rotation, quaternion.Euler(30,0,0));
                    dir = math.mul(rotation, dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;*/
                    /*
                    //Rotates the camera to look at the lookAtPoint.
                    float3 direction = movementData.cameraLookAtPoint - trans.Value;
                    rot.Value = quaternion.LookRotation(math.normalize(direction), Vector3.up);
                    */
                    //maybe we only copy the y axis rotation
                    /*float3 dir = trans.Value - movementData.cameraLookAtPoint; //direction relative to pivot
                    //take only the y and the z, then rotate around the x maybe.
                    quaternion endme = quaternion.Euler(rotationDegrees, rot.Value.value.y, rot.Value.value.z);
                    //quaternion killme = math.mul(quaternion.Euler(rotationDegrees,0,0), rot.Value);
                    dir = math.mul(endme, dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;*/
                    /*****************************THIS ONE SHOULD WORK BUT DOESN'T***********************************
                    var dir: Vector3 = point - pivot; // get point direction relative to pivot
                    dir = Quaternion.Euler(angles) * dir; // rotate it
                    point = dir + pivot; // calculate rotated point
                    return point; // return it
                    except this doesn't work at all, actually, for some reason. it should??? I see zero reason why it wouldn't. there should be no problems happening here.
                    *************************************************************************************************/
                    //we want to take the camera's quaternion, rotate it by (45) degrees, then multiply that to our vector3. then add that to our pivot point.
                    //none of the below even begins to work btw
                    /*quaternion guy = math.mul(rot.Value, quaternion.Euler(rotationDegrees,0,0));
                    rotationData.targetPosition = math.mul(guy, (trans.Value - movementData.cameraLookAtPoint));*/
                    /*float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    dir = math.mul(quaternion.Euler(rotationDegrees,rot.Value.value.y,0), dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;*/
                    /*quaternion test = math.mul(rot.Value, quaternion.Euler(rotationDegrees,0,0));
                    float3 dir = math.normalize(trans.Value - movementData.cameraLookAtPoint);
                    dir = math.mul(test, dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;*/
                }
                rotationData.rotating = true;
            }
            else if (rotationData.rotating) //still rotating to the target position
            {
                Vector3 closeOrNaw = trans.Value - rotationData.targetPosition;
                if (closeOrNaw.magnitude < rotationData.speed) //TODO: see if this is dumb or not? it probably is tbh.
                {
                    trans.Value = rotationData.targetPosition;
                    rotationData.rotating = false;
                }
                else
                {
                    float3 moveDir = math.normalize(rotationData.targetPosition - trans.Value);
                    trans.Value += moveDir * rotationData.speed;
                }
            }
            
            //Rotates the camera to look at the lookAtPoint.
            float3 direction = movementData.cameraLookAtPoint - trans.Value;
            rot.Value = quaternion.LookRotation(math.normalize(direction), Vector3.up);
            
        }).Run();
        return default;
    }
}