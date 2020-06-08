using Reactics.Commons;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
namespace Reactics.Battle.Map.Authoring
{

    [RequiresEntityConversion]
    [ConverterVersion("Nero", 1)]
    public class MapBody : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        public Point position;
        [SerializeField]
        public Unit unit;

        private void OnValidate()
        {
            if (gameObject.GetComponentInParent<Map>(true) == null)
                throw new UnityException($"{name} must be child of {typeof(Map)}");
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (dstManager.TryGetComponent<Parent>(entity, out Parent parent) && dstManager.HasComponent<MapData>(parent.Value))
            {
                EntityQuery unitManagerQuery = dstManager.CreateEntityQuery(typeof(UnitManagerData));
                var unitManagerArray = unitManagerQuery.ToEntityArray(Allocator.TempJob); //there should only be one... maybe two actually. for right now this is fine.
                dstManager.AddComponentData(entity, new Reactics.Battle.Map.MapBody
                {
                    point = position,
                    anchor = new Anchor3D(0,1,0)
                });
                dstManager.AddComponent<MapCollidableData>(entity);
                dstManager.AddComponentData(entity, new MapElement { value = parent.Value });
                //dstManager.RemoveComponent<Translation>(entity);
                conversionSystem.ConfigureEditorRenderData(entity, gameObject, false);
                dstManager.AddComponentData(entity, new FindingPathInfo
                {
                    destination = new Point(10, 10),
                    speed = 8,
                    maxElevationDifference = 1
                });
                dstManager.AddComponentData(entity, new ActionMeter{
                    rechargeRate = 10f,
                    chargeable = true,
                    charge = 100f
                });
                dstManager.AddComponentData(entity, new UnitCommand{
                    unitManagerEntity = unitManagerArray[0]
                });
                dstManager.AddComponentData(entity, unit.CreateComponent());
                //dstManager.AddComponentData(entity, new MoveTilesTag());
                dstManager.AddBuffer<HighlightTile>(entity);
                dstManager.AddBuffer<EffectBuffer>(entity);

                unitManagerArray.Dispose();
            }
        }
    }
}