using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Entities;

namespace Reactics.Core.Effects {
    public struct PointTarget {
        public Point value;
    }
    public struct MapBodyTarget {
        public MapBody mapBody;
        public Entity entity;
    }
    public struct DirectionTarget {
        public MapBodyDirection value;
    }
}