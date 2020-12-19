using NeroWeNeed.Commons;
using Reactics.Core.Commons;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
namespace Reactics.Core.Map.Authoring {

    public class MapBody : MonoBehaviour, IConvertGameObjectToEntity {
        [SerializeField]
        public Point position;

        private void OnValidate() {
            if (gameObject.GetComponentInParent<Map>(true) == null)
                throw new UnityException($"{name} must be child of {typeof(Map)}");
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            if (dstManager.TryGetComponent<Parent>(entity, out Parent parent) && dstManager.HasComponent<MapData>(parent.Value)) {
                //EntityQuery unitManagerQuery = dstManager.CreateEntityQuery(typeof(UnitManagerData));
                //var unitManagerArray = unitManagerQuery.ToEntityArray(Allocator.TempJob); //there should only be one... maybe two actually. for right now this is fine.
                dstManager.AddComponentData(entity, new Core.Map.MapBody
                {
                    point = position,
                    anchor = new Anchor(0, 1, 0)
                });
                dstManager.AddComponent<MapCollidableData>(entity);
                dstManager.AddComponentData(entity, new MapElement { value = parent.Value });
                //dstManager.RemoveComponent<Translation>(entity);
                conversionSystem.ConfigureEditorRenderData(entity, gameObject, false);

            }
        }
    }
}