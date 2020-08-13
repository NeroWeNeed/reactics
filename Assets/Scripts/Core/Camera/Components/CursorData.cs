using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Entities;
using Unity.Mathematics;
namespace Reactics.Core.Camera {
    [GenerateAuthoringComponent]
    public struct CursorData : IComponentData {
        public Entity cameraEntity;
        public Point currentHoverPoint;
        public float3 rayOrigin;
        public float3 rayDirection;
        public float rayMagnitude;
        public float tileSize;
    }
}