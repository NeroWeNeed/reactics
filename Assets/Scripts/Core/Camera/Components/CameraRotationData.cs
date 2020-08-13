using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Reactics.Core.Camera {
    [GenerateAuthoringComponent]
    public struct CameraRotationData : IComponentData {
        public Vector2 rotationDirection;
        public float3 targetPosition;
        public float3 lastPosition;
        public float speed;
        public int horizontalAngles;
        public int verticalAngles;
        public bool rotating;
        public float rotationTime;
    }
}