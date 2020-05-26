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

[UpdateInGroup(typeof(RenderingSystemGroup))]
[UpdateAfter(typeof(CameraMovementSystem))]
public class CameraRotationSystem : SystemBase //change this to OrbitSystem, perhaps
{
    protected override void OnUpdate() 
    {
        float deltaTime = Time.DeltaTime;
        
        Entities.ForEach((ref Translation trans, ref Rotation rot, ref CameraRotationData rotData, in CameraMovementData moveData) => 
        {
            if (!rotData.rotating && rotData.rotationDirection.magnitude > 0.1f) //if not already rotating and an input is detected
            {
                float rotationDegrees = 0f;
                if (rotData.rotationDirection.x > 0.1f)
                {
                    rotationDegrees = -math.radians(360f/rotData.horizontalAngles);
                    float3 dir = trans.Value - moveData.cameraLookAtPoint;
                    dir = math.mul(quaternion.Euler(0,rotationDegrees,0), dir);
                    rotData.targetPosition = dir + moveData.cameraLookAtPoint;
                }
                else if (rotData.rotationDirection.x < -0.1f)
                {
                    rotationDegrees = math.radians(360f/rotData.horizontalAngles);
                    float3 dir = trans.Value - moveData.cameraLookAtPoint;
                    dir = math.mul(quaternion.Euler(0,rotationDegrees,0), dir);
                    rotData.targetPosition = dir + moveData.cameraLookAtPoint;
                }
                /*(remember when subtracting vectors a-b is pointing from a to b)
                TODO: When rotating around the z axis, the rotation of the lookatpoint hasn't changed, so we just rotate around it's z, despite our current rotation.
                figure out a way to rotate that as well whenever the camera rotates maybe, so they're always facing the same direction.
                Rotating around the Y was never an issue because that's basically just always 0 for our purposes. But we may as well change that to be proper eventually.
                */
                else if (rotData.rotationDirection.y > 0.1f)
                {
                    //float3 upwardDirection = math.mul(rot.Value, new float3(0, 0, data.speed));
                    //upwardDirection.y = 0;
                    rotationDegrees = math.radians(180f/rotData.verticalAngles);
                    //do something once it works
                    float3 dir = trans.Value - trans.Value;
                    quaternion killme = math.mul(quaternion.Euler(math.radians(30),0,0), rot.Value);
                    dir = math.mul(killme, dir);
                    rotData.targetPosition = dir + trans.Value;
                }
                else if (rotData.rotationDirection.y < -0.1f)
                {
                    rotationDegrees = -math.radians(180f/rotData.verticalAngles);
                    //float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    float3 normVec = math.normalize(trans.Value); //maybe this will do something
                    normVec = math.mul(quaternion.Euler(math.radians(30), math.radians(rot.Value.value.y), 0), normVec);
                    rotData.targetPosition = normVec * (moveData.offsetValue * moveData.zoomMagnitude);
                    /*var dir: Vector3 = point - pivot; // get point direction relative to pivot
                    dir = Quaternion.Euler(angles) * dir; // rotate it
                    point = dir + pivot; // calculate rotated point
                    return point; // return it
                    except this doesn't work at all, actually, for some reason. it should??? I see zero reason why it wouldn't. there should be no problems happening here.*/
                    //let's first try to get the rotation we want. how about that.
                    /*
                    public static float3 RotateAroundPoint(float3 position, float3 pivot, float3 axis, float delta) =>
                    math.mul(quaternion.axisAngle(axis, delta), position - pivot) + pivot;
                    */
                    /*trans.Value = math.mul(quaternion.Euler(math.radians(30), 0, 0), trans.Value);
                    rotationData.targetPosition = trans.Value;*/
                    /*float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    dir = math.mul(quaternion.Euler(0,rotationDegrees,0), dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;*/
                    /*THIS IS CLOSE?
                    float3 normVec = math.normalize(trans.Value); //maybe this will do something
                    normVec = math.mul(quaternion.Euler(math.radians(30), 0, 0), normVec);
                    rotationData.targetPosition = normVec * (movementData.offsetValue * movementData.zoomMagnitude);*/
                    //trans.value = math.normalize(trans.Value - data.cameraLookAtPoint) * (data.offsetValue * data.zoomMagnitude) + data.cameraLookAtPoint;
                    /*
                    Rotate around a local axis: rot.Value = rot.Value * Quaternion.AngleAxis(10, Vector3.Up);
                    Rotate around a world axis: rot.Value = Quaternion.AngleAxis(10, Vector3.Up) * rot.Value;

                    Quaternion rotR = Quaternion.AngleAxis(10 * Time.deltaTime, Vector3.right);
                    Quaternion rotU = Quaternion.AngleAxis(10 * Time.deltaTime, Vector3.up);
                    
                    // rotate around World Right
                    transform.rotation = rotR * transform.rotation;
                    // rotate around Local Up
                    transform.rotation = transform.rotation * rotU;
                    */
                    //ok yup that was my last idea it's too late for this garbage I'll be back later
                    /*this makes no sense
                    quaternion why = math.mul(rot.Value, quaternion.Euler(math.radians(30), 0, 0, math.RotationOrder.Default));
                    //quaternion.RotateX(30);
                    rotationData.targetPosition = math.mul(why, (trans.Value - movementData.cameraLookAtPoint)) + movementData.cameraLookAtPoint;*/
                    //this is rotating around a fix axis. so we need to somehow base it on our rotation.
                    /*float3 dir = trans.Value - movementData.cameraLookAtPoint;
                    quaternion killme = quaternion.Euler(math.radians(30), 0, 0);
                    dir = math.mul(killme, dir);
                    rotationData.targetPosition = dir + movementData.cameraLookAtPoint;*/
                    //return rotation * (vector - pivot) + pivot;
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
                rotData.lastPosition = trans.Value;
                rotData.rotationTime = 0f;
                rotData.rotating = true;
            }
            else if (rotData.rotating && rotData.rotationTime < 1) //still rotating to the target position
            {
                rotData.rotationTime += deltaTime * rotData.speed;
                trans.Value = Vector3.Lerp(rotData.lastPosition, rotData.targetPosition, rotData.rotationTime);
            }
            else
            {
                rotData.rotating = false;
            }
            /*
            public class ExampleClass : MonoBehaviour
{
    // Transforms to act as start and end markers for the journey.
    public Transform startMarker;
    public Transform endMarker;

    // Movement speed in units per second.
    public float speed = 1.0F;

    // Time when the movement started.
    private float startTime;

    // Total distance between the markers.
    private float journeyLength;

    void Start()
    {
        // Keep a note of the time the movement started.
        startTime = Time.time;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(startMarker.position, endMarker.position);
    }

    // Move to the target end position.
    void Update()
    {
        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - startTime) * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.Lerp(startMarker.position, endMarker.position, fractionOfJourney);
    }
}
            */
            //Rotates the camera to look at the lookAtPoint.
            float3 direction = moveData.cameraLookAtPoint - trans.Value;
            rot.Value = quaternion.LookRotation(math.normalize(direction), Vector3.up);
            
        }).Run();
    }
}