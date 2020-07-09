using Reactics.Battle.Map;
using Reactics.Commons;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Battle
{
    public struct SampleEffect : IEffect<Point>
    {
        public MapLayer layer;

        public float value;

        public bool otherValue;
        [SerializeField]
        public BlittableAssetReference64 item;

        public void Invoke(Entity cursorEntity, Entity effectDataEntity, Entity sourceEntity, MapBody source, Entity mapEntity, MapData map, Point target, EntityCommandBuffer entityCommandBuffer)
        {

        }
    }
}