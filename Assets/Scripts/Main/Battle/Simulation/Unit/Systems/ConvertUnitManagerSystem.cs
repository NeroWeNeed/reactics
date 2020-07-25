using Reactics.Battle;
using Reactics.Battle.Map;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertUnitManagerSystem : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new UnitManagerData
        {
            commanding = false
        });
#if UNITY_EDITOR
        dstManager.SetName(entity, "UnitManager");
#endif
    }
}
