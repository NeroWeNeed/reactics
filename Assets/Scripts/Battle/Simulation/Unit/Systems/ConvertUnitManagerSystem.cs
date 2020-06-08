using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Reactics.Battle;
using Unity.Collections;
using Reactics.Battle.Map;

[RequiresEntityConversion]
public class ConvertUnitManagerSystem : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new UnitManagerData
        {
            commanding = false
        });
        dstManager.SetName(entity, "UnitManager");
    }
}
