using Unity.Entities;
using Unity.Mathematics;
using Reactics.Battle;

[GenerateAuthoringComponent]
public struct CursorData : IComponentData
{
    public Entity cameraEntity;
    public Point currentHoverPoint;
    public float3 rayOrigin;
    public float3 rayDirection;
    public float rayMagnitude;
    public float tileSize;
    //public string controlScheme; //may be better as a shared component... also apparently strings aren't allowed? weird.
}