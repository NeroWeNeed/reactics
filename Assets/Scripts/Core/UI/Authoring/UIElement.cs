using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Reactics.Core.UI.Author {

    [RequiresEntityConversion]
    [ConverterVersion("Nero", 2)]
    public class UIElement : MonoBehaviour, IConvertGameObjectToEntity {
        private Mesh mesh;

        public Material material;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            UIElement component = null;
            if (transform.parent?.TryGetComponent(out component) == true) {
                dstManager.AddComponentData(entity, new UIParent
                {
                    value = conversionSystem.GetPrimaryEntity(component)
                });
            }
            dstManager.AddComponent<UILayoutVersion>(entity);
            dstManager.AddComponent<UIMeshVersion>(entity);
            dstManager.AddComponent<UIResolvedBox>(entity);
            dstManager.AddComponent<RenderMesh>(entity);
            var mesh = GetMeshData(out int subMesh);
            dstManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                subMesh = subMesh,
                material = material,
                layer = UIScreenInfoSystem.UI_LAYER,
                castShadows = ShadowCastingMode.Off,
                receiveShadows = false,
                needMotionVectorPass = false
            });
            dstManager.AddComponent<LocalToWorld>(entity);
            dstManager.AddComponent<RenderBounds>(entity);
            dstManager.AddComponent<LocalToScreen>(entity);
            dstManager.AddBuffer<UIMeshVertexData>(entity);
            dstManager.AddBuffer<UIMeshIndexData>(entity);
            var children = new NativeList<UIChild>(Allocator.Temp);
            foreach (Transform child in transform) {
                if (child.TryGetComponent(out component)) {
                    children.Add(conversionSystem.GetPrimaryEntity(component));
                }
            }
            var buffer = dstManager.AddBuffer<UIChild>(entity);
            buffer.AddRange(children.AsArray());
        }
        private Mesh GetMeshData(out int subMesh) {
            return GetMeshData(this.transform, out subMesh, 0);
        }
        private Mesh GetMeshData(Transform transform, out int subMesh, int offset = 0) {
            if (transform.parent == null) {
                var element = transform.GetComponent<UIElement>();
                if (element.mesh == null) {
                    element.mesh = new Mesh
                    {
                        name = $"{element.name} UI Mesh"
                    };
                    element.mesh.MarkDynamic();
                }
                subMesh = offset + 1;
                return element.mesh;
            }
            else {
                return GetMeshData(transform.parent, out subMesh, offset + (this.transform == transform ? this.transform.GetSiblingIndex() + 1 : transform.parent.childCount));
            }
        }
    }


}