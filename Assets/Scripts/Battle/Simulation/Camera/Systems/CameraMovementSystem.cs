using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Reactics.Battle;

[UpdateInGroup(typeof(RenderingSystemGroup))]
public class CameraMovementSystem : SystemBase
{
    protected override void OnUpdate() 
    {
        float deltaTime = Time.DeltaTime;
        
        Entities.ForEach((ref Translation trans, ref CameraMovementData moveData, in Rotation rot, in CameraRotationData rotData, in CameraMapData mapData, in ControlSchemeData controlSchemeData) => 
        {
            bool moving = false;
            if (!moveData.returnToPoint && !rotData.rotating) //delete this if you want to move/zoom and rotate at the same time.
            {
                if (math.abs(moveData.gridMovementDirection.x) > 0.8f || math.abs(moveData.gridMovementDirection.y) > 0.8f) //ignore diagonals
                {
                    moveData.dummyBuffer++;
                    moveData.dummyBuffer %= 15;
                    
                    if (moveData.dummyBuffer == 1)
                    {//TODO: Make this button hold preventative measure less dumb
                    float3 oldTranslation = trans.Value;
                    float3 oldLookAtPoint = moveData.cameraLookAtPoint;
                    float3 upwardDirection = new float3(0,0,0);
                    Quaternion rotation = rot.Value;
                    //This means we're not looking in the cardinal directions, so we gotta rotate the input a little.
                    if (rotation.eulerAngles.y % 90f > 1f) 
                    {
                        //Rotate our input relative to the camera's current rotation away from a cardinal direction
                        //Upward inputs should always be NE despite our rotation.
                        float rotationInRadians = (rotation.eulerAngles.y % 90) * (math.PI/180f);
                        quaternion rotationAmount = math.mul(rot.Value, quaternion.AxisAngle(Vector3.up,rotationInRadians));
                        upwardDirection = math.mul(rotationAmount, new float3(0,0,mapData.tileSize));
                    }
                    else
                    {
                        upwardDirection = math.mul(rot.Value, new float3(0,0,mapData.tileSize));
                    }
                    upwardDirection.y = 0;

                    //positive x is down, positive z is to the right (not always)
                    if (moveData.gridMovementDirection.y > 0.1f) //UP INPUT
                    {
                        trans.Value += upwardDirection;
                        moveData.cameraLookAtPoint += upwardDirection;
                    }
                    if (moveData.gridMovementDirection.x > 0.1f) //RIGHT INPUT
                    {
                        float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                        trans.Value += val;
                        moveData.cameraLookAtPoint += val;
                    }
                    if (moveData.gridMovementDirection.x < -0.1f) //LEFT INPUT
                    {
                        float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                        trans.Value -= val;
                        moveData.cameraLookAtPoint -= val;
                    }
                    if (moveData.gridMovementDirection.y < -0.1f) //DOWN INPUT
                    {
                        trans.Value -= upwardDirection;
                        moveData.cameraLookAtPoint -= upwardDirection;
                    }
                    if (moveData.cameraLookAtPoint.x < 0 || moveData.cameraLookAtPoint.z < 0 ||
                        moveData.cameraLookAtPoint.x > mapData.mapWidth || moveData.cameraLookAtPoint.z > mapData.mapLength)
                    {
                        trans.Value = oldTranslation;
                        moveData.cameraLookAtPoint = oldLookAtPoint;
                    }
                    }
                }
                else
                {
                    //Reset dummy buffer
                    moveData.dummyBuffer = 0;
                    //Panning based movement
                    //TODO: Vector math stuff (confirm whether the magnitude is the same for any given rotation... it probably isn't
                    //TODO: Have an actual variable for ScreenEdgeLength instead of passing in stuff.
                    float3 upwardDirection = math.mul(rot.Value, new float3(0, 0, moveData.speed * deltaTime));
                    upwardDirection.y = 0;

                    if (moveData.panMovementDirection.y > 0.1f) //UP INPUT
                    {
                        float3 newLookAtPoint = moveData.cameraLookAtPoint + upwardDirection;
                        if (newLookAtPoint.x > 0 && newLookAtPoint.z > 0 &&
                            newLookAtPoint.x < mapData.mapWidth && newLookAtPoint.z < mapData.mapLength)
                        {
                            trans.Value += upwardDirection;
                            moveData.cameraLookAtPoint += upwardDirection;
                        }
                        moving = true;
                    }
                    if (moveData.panMovementDirection.x > 0.1f) //RIGHT INPUT
                    {
                        float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                        float3 newLookAtPoint = moveData.cameraLookAtPoint + val;
                        if (newLookAtPoint.x > 0 && newLookAtPoint.z > 0 &&
                            newLookAtPoint.x < mapData.mapWidth && newLookAtPoint.z < mapData.mapLength)
                        {
                            trans.Value += val;
                            moveData.cameraLookAtPoint += val;
                        }
                        moving = true;
                    }
                    if (moveData.panMovementDirection.x < -0.1f) //LEFT INPUT
                    {
                        float3 val = math.mul(quaternion.AxisAngle(Vector3.up, math.PI/2), upwardDirection);
                        float3 newLookAtPoint = moveData.cameraLookAtPoint - val;
                        if (newLookAtPoint.x > 0 && newLookAtPoint.z > 0 &&
                            newLookAtPoint.x < mapData.mapWidth && newLookAtPoint.z < mapData.mapLength)
                        {
                            trans.Value -= val;
                            moveData.cameraLookAtPoint -= val;
                        }
                        moving = true;
                    }
                    if (moveData.panMovementDirection.y < -0.1f) //DOWN INPUT
                    {
                        float3 newLookAtPoint = moveData.cameraLookAtPoint - upwardDirection;
                        if (newLookAtPoint.x > 0 && newLookAtPoint.z > 0 &&
                            newLookAtPoint.x < mapData.mapWidth && newLookAtPoint.z < mapData.mapLength)
                        {
                            trans.Value -= upwardDirection;
                            moveData.cameraLookAtPoint -= upwardDirection;
                        }
                        moving = true;
                    }
                }
            }
            if (!moving && !rotData.rotating && controlSchemeData.currentControlScheme == ControlSchemes.Gamepad)
                {
                    //ok so tilesize isn't in ecs anywhere, ask about that one, but for now here's tile snapping yaaaay
                    //this will probably end up going in another system with some kind of map component data... but for now it's here. oops.
                    float3 oldLookAtPoint = moveData.cameraLookAtPoint;
                    
                    if (!moveData.returnToPoint)
                    {
                        //Put the cam look at point in the center of the tile
                        moveData.cameraLookAtPoint.x = (moveData.cameraLookAtPoint.x - moveData.cameraLookAtPoint.x % mapData.tileSize + mapData.tileSize/2);
                        moveData.cameraLookAtPoint.z = (moveData.cameraLookAtPoint.z - moveData.cameraLookAtPoint.z % mapData.tileSize + mapData.tileSize/2);
                    }
                    else
                    {
                        //Put the cam look at point back on the currently selected unit
                        moveData.cameraLookAtPoint.x = moveData.returnPoint.x * mapData.tileSize + mapData.tileSize/2;
                        moveData.cameraLookAtPoint.z = moveData.returnPoint.y * mapData.tileSize + mapData.tileSize/2;
                        moveData.returnToPoint = false;
                    }

                    //Offset the camera by the exact same amount
                    trans.Value += (moveData.cameraLookAtPoint - oldLookAtPoint);
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
        }).Run();
    }
}