using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct CameraRotationData : IComponentData
{
    public Vector2 rotationDirection;
    public float3 targetPosition;
    public float speed;
    public int horizontalAngles;
    public int verticalAngles;
    public bool lockToHalfVerticalSphere; //admittedly this is stupid, and also it's more like a fourth of a sphere. so i'm gonna remove this.
    public bool rotating;
}