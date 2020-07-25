using Unity.Entities;
using UnityEngine;

namespace Reactics.Battle.Map
{
    [UpdateInGroup(typeof(MapBodyManagementSystemGroup))]
    public class MapBodyCollisionStateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithChangeFilter<MapBody>().ForEach((ref MapCollidableData data, in MapBody body) =>
            {
                data.point = body.point;
            }).Schedule();
        }
    }
}