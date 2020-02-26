using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Reactics.Battle;

[GenerateAuthoringComponent]
public struct CursorData : IComponentData
{
    public Entity cameraEntity;
    public Point lastHoverPoint;
    public float3 rayOrigin;
    public float3 rayDirection;
    public float rayMagnitude;
    public Entity map;
    //public string controlScheme; //may be better as a shared component... also apparently strings aren't allowed? weird.
}