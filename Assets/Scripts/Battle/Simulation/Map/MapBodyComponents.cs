using Reactics.Commons;
using Unity.Entities;
using Unity.Transforms;

namespace Reactics.Battle.Map
{
    [WriteGroup(typeof(LocalToWorld))]
    public struct MapBody : IComponentData
    {
        public Point point;
        public MapBodyDirection direction;
        public Anchor3D anchor;
    }



    public enum MapBodyDirection
    {
        
        Uninitialized = 0,
        West = 1,
        NorthWest = 2,
        North = 3,
        NorthEast = 4,
        East = 5,
        SouthEast = 6,
        South = 7,
        SouthWest = 8

    }
    public enum MapBodyAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        Bottom,
        BottomRight
    }


    public struct FindingPathInfo : IComponentData
    {
        public Point destination;

        public float speed;

        public int maxElevationDifference;


    }
    [WriteGroup(typeof(LocalToWorld))]
    public struct MapBodyPathFindingRoute : IBufferElementData
    {
        public Point next;
        public Point previous;
        public float speed;
        public float completion;

        public MapBodyPathFindingRoute(Point next, Point previous, float speed)
        {
            this.next = next;
            this.previous = previous;
            this.speed = speed;
            this.completion = 0;
        }
    }

}