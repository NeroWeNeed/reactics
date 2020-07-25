using Unity.Entities;
using Reactics.Battle.Map;
using Unity.Collections;

namespace Reactics.Battle
{
    public struct MapBodyDistanceTargetFilter : ITargetFilter<MapBodyTarget>
    {
        public int distance;

        public void Filter(Entity entitySourceBody, MapBody sourceBody, Entity entityMap, MapData map, NativeList<MapBodyTarget> targets)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].mapBody.point.ManhattanDistance(sourceBody.point) < distance)
                {
                    targets.RemoveAtSwapBack(i);
                    i--;
                }
            }
        }
    }
}