using Reactics.Core.Battle;
using Reactics.Core.Map;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace Reactics.Core.Unit.Authoring {
    [RequiresEntityConversion]
    public class UnitManager : MonoBehaviour, IConvertGameObjectToEntity {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
            EntityQuery mapQuery = dstManager.CreateEntityQuery(typeof(MapRenderInfo));
            var mapEntity = mapQuery.GetSingletonEntity();
            dstManager.AddComponentData(entity, new UnitManagerData
            {
                commanding = false
            });
            dstManager.AddComponentData(entity, new MapElement
            {
                value = mapEntity
            });
#if UNITY_EDITOR
            dstManager.SetName(entity, "UnitManager");
#endif
        }
    }
}