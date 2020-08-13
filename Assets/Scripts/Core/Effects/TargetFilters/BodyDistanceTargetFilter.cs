using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;

namespace Reactics.Core.Effects {
    public struct MapBodyDistanceTargetFilter : ITargetFilter<MapBodyTarget> {
        public int distance;

        public void Filter(Entity entitySourceBody, MapBody sourceBody, Entity entityMap, MapData map, NativeList<MapBodyTarget> targets) {
            for (int i = 0; i < targets.Length; i++) {
                if (targets[i].mapBody.point.ManhattanDistance(sourceBody.point) < distance) {
                    targets.RemoveAtSwapBack(i);
                    i--;
                }
            }
        }
    }
}