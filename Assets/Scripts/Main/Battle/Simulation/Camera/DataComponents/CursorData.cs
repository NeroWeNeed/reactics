using Unity.Entities;
using Unity.Mathematics;
using Reactics.Battle;
using Reactics.Battle.Map;

[GenerateAuthoringComponent]
public struct CursorData : IComponentData
{
    public Entity cameraEntity;
    public Point currentHoverPoint;
    public float3 rayOrigin;
    public float3 rayDirection;
    public float rayMagnitude;
    public float tileSize;
}