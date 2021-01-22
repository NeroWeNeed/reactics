using Unity.Entities;
using UnityEngine;
namespace NeroWeNeed.UIDots {

    public class UICursorObject : MonoBehaviour, IConvertGameObjectToEntity {
        [SerializeField]
        public UIObject target;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            if (target != null) {
                Entity uiGraphEntity;
                BlobAssetReference<UIGraphOld> graph;
                uiGraphEntity = conversionSystem.GetPrimaryEntity(target);
                graph = dstManager.GetComponentData<UIRoot>(uiGraphEntity).graph;
                //dstManager.AddComponentData(entity, new UICursor { target = uiGraphEntity, index = graph.GetFirstSelectableIndex(); });
                dstManager.AddComponent<UICursorInput>(entity);
                dstManager.AddComponent<UICursorDirty>(entity);
            }
        }
    }
}