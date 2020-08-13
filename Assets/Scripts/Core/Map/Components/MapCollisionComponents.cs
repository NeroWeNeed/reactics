using System;
using Reactics.Core.Commons;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
namespace Reactics.Core.Map {
    [Serializable]
    public struct MapCollisionState : IComponentData {
        public UnsafeHashMap<Point, Entity> value;
    }
    public struct MapCollidableData : IComponentData {
        public Point point;
    }

    public struct MapCollidableSystemData : ISystemStateComponentData {
        public Point point;
    }
}