using Reactics.Battle.Map;
using Unity.Entities;

namespace Reactics.Battle
{
    public struct SampleEffect : IEffect<Point>
    {
        public MapLayer layer;

        public float value;

        public bool otherValue;


        public void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, Point target, EntityCommandBuffer entityCommandBuffer)
        {
            
        }
    }
}