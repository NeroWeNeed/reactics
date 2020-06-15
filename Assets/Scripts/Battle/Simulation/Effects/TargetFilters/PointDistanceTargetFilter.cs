using Unity.Entities;
using Reactics.Battle.Map;
using Unity.Collections;

namespace Reactics.Battle
{
    public struct PointDistanceTargetFilter : ITargetFilter<Point>
    {


        public int distance;
        public void Filter(Entity entitySourceBody, MapBody sourceBody, Entity entityMap, MapData map, NativeList<Point> targets)
        {
            

            for (int i = 0; i < targets.Length; i++)
            {

                if (targets[i].ManhattanDistance(sourceBody.point) < distance)
                {
                    targets.RemoveAtSwapBack(i);
                    i--;
                }
            }
        }
    }
}