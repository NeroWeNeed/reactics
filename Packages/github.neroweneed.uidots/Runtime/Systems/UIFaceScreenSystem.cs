using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace NeroWeNeed.UIDots {
    [UpdateInGroup(typeof(UISystemGroup))]
    public class UIFaceScreenSystem : SystemBase {
        protected override void OnUpdate() {
            Entities.WithAll<UIFaceScreen>().ForEach((Entity entity, ref LocalToWorld ltw, in UICameraContext contextData) =>
            {
                var cameraPosition = math.transform(contextData.cameraLTW, float3.zero);
                var cameraRotation = new quaternion(contextData.cameraLTW);
                var scale = HasComponent<Scale>(entity) ? GetComponent<Scale>(entity).Value : 1;
                var rotation = HasComponent<Rotation>(entity) ? GetComponent<Rotation>(entity).Value : quaternion.identity;
                var position = HasComponent<Translation>(entity) ? GetComponent<Translation>(entity).Value : ltw.Position;
                ltw.Value = float4x4.TRS(position, math.mul(rotation,cameraRotation), scale);
            }).WithoutBurst().Run();
        }
    }
}