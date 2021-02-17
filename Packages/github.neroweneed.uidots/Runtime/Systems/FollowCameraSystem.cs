using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    public class FollowCameraSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.ForEach((ref LocalToWorld ltw, in FollowCameraData data) =>
            {
                ltw.Value = EntityManager.GetComponentData<LocalToWorld>(data.value).Value;
            }).WithoutBurst().Run();
        }
    }



}