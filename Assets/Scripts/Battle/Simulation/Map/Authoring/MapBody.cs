using Reactics.Commons;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
namespace Reactics.Battle.Map.Authoring
{

    [RequiresEntityConversion]
    [ConverterVersion("Nero", 1)]
    public class MapBody : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        public Point position;

        private void OnValidate()
        {
            if (gameObject.GetComponentInParent<Map>(true) == null)
                throw new UnityException($"{name} must be child of {typeof(Map)}");
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (dstManager.TryGetComponent<Parent>(entity, out Parent parent) && dstManager.HasComponent<MapData>(parent.Value))
            {
                dstManager.AddComponentData(entity, new Reactics.Battle.Map.MapBody
                {
                    point = position,
                    anchor = new Anchor3D(0,1,0)
                });
                dstManager.AddComponent<MapCollidableData>(entity);
                dstManager.AddComponentData(entity, new MapElement { value = parent.Value });
                dstManager.RemoveComponent<Translation>(entity);
                conversionSystem.ConfigureEditorRenderData(entity, gameObject, false);
                dstManager.AddComponentData(entity, new FindingPathInfo
                {
                    destination = new Point(10, 10),
                    speed = 8,
                    maxElevationDifference = 1
                });
            }
        }
    }
}