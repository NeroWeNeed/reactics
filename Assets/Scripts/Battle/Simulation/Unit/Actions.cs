using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Reactics.Battle
{

    public interface IActionTarget<T> where T : struct
    {
        float3 WorldPosition { get; }

        T Value { get; }
    }
    public struct MapBodyActionTarget : IActionTarget<MapBody>
    {
        private float3 worldPosition;
        public float3 WorldPosition => worldPosition;
        private MapBody mapBody;
        public MapBody Value => mapBody;

        public MapBodyActionTarget(MapBody mapBody, float3 worldPosition)
        {
            this.mapBody = mapBody;
            this.worldPosition = worldPosition;
        }
    }

    public struct MapTileActionTarget : IActionTarget<Point>
    {
        private float3 worldPosition;
        public float3 WorldPosition => worldPosition;

        private Point point;
        public Point Value => point;

        public MapTileActionTarget(Point point, float3 worldPosition)
        {
            this.point = point;
            this.worldPosition = worldPosition;
        }
    }

    public interface IActionTargetFilter<T> where T : struct
    {
        NativeArray<R> Filter<R>(Entity initiator,NativeArray<T> source, Allocator allocator = Allocator.Temp) where R : struct, IActionTarget<T>;
    }

    public interface IAction<T> where T : struct {

        void Invoke(Entity entity,EntityManager entityManager, T target);
    }

    //not sure what your plan was with this stuff but maybe it's better than mine which I did down here
    //These should have dynamic buffers for storing affected tiles maybe.
    /*public struct Action : IComponentData
    {
        /*public ushort magnitude;
        public bool damage;
        public ushort range;
        public float cost;
        public Entity targetedUnit;
        public Point targetTile;
        public Effect effect;
        public aoeEffect
        //aoe range go here
        //public bool aoe;
        //public ushort aoeRange;
    }*/
}