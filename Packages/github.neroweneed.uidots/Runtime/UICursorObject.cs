using Unity.Entities;
using UnityEngine;
namespace NeroWeNeed.UIDots {

    public class UICursorObject : MonoBehaviour, IConvertGameObjectToEntity {
        [SerializeField]
        public UIObject target;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            Entity uiGraphEntity;
            BlobAssetReference<UIGraph> graph;
            if (target == null) {
                uiGraphEntity = Entity.Null;
                graph = default;
            }
            else {
                uiGraphEntity = conversionSystem.GetPrimaryEntity(target);
                graph = dstManager.GetComponentData<UIRoot>(uiGraphEntity).graph;
            }
            if (graph.IsCreated) {
                dstManager.AddComponentData(entity, new UICursor { target = uiGraphEntity, index = UIGraphUtility.GetFirstSelectableIndex(graph) });
            }
            else {
                dstManager.AddComponentData(entity, new UICursor { target = Entity.Null, index = -1 });
            }
            dstManager.AddComponent<UICursorInput>(entity);
            dstManager.AddComponent<UICursorDirty>(entity);


        }
    }
}