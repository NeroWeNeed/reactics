using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;
namespace Reactics.Core.Effects {
    public struct PointDistanceTargetFilter : ITargetFilter<Point> {


        public int distance;
        public void Filter(Entity entitySourceBody, MapBody sourceBody, Entity entityMap, MapData map, NativeList<Point> targets) {


            for (int i = 0; i < targets.Length; i++) {

                if (targets[i].ManhattanDistance(sourceBody.point) < distance) {
                    targets.RemoveAtSwapBack(i);
                    i--;
                }
            }
        }
    }
}