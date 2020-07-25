using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Reactics.Battle.Map
{
    public struct MapCollisionState : IComponentData
    {
        public UnsafeHashMap<Point, Entity> value;
    }
    public struct MapCollidableData : IComponentData
    {
        public Point point;
    }

    public struct MapCollidableSystemData : ISystemStateComponentData
    {
        public Point point;
    }
}