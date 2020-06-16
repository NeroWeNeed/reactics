
using Unity.Entities;
using Unity.Mathematics;
using Reactics.Battle;
using Reactics.Battle.Map;

[GenerateAuthoringComponent]
public struct CameraMovementData : IComponentData
{
    public int dummyBuffer;
    public float2 panMovementDirection;
    public float2 gridMovementDirection;
    public float speed;
    public float offsetValue;
    public float3 cameraLookAtPoint;
    public float zoomMagnitude; //should always be above 0
    public float zoomDirectionAndStrength;
    public float lowerZoomLimit;
    public float upperZoomLimit;
    public Point returnPoint;
    public bool returnToPoint;
}