using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.Map {
    [UpdateInGroup(typeof(MapBodyManagementSystemGroup))]
    public class MapBodyCollisionStateSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithChangeFilter<MapBody>().ForEach((ref MapCollidableData data, in MapBody body) =>
            {
                data.point = body.point;
            }).Schedule();
        }
    }
}