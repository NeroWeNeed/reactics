using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct CameraMovementData : IComponentData
{
    public float2 movementDirection;
    public int speed;
    public float3 cameraLookAtPoint;
    public float zoomMagnitude; //should always be above 0
    public float zoomDirectionAndStrength;
    public float lowerZoomLimit;
    public float upperZoomLimit;
}