using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Reactics.Battle;

[RequiresEntityConversion]
public class ConvertInputSystem : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new InputData
        {
            currentActionMap = ActionMaps.BattleControls,
                previousActionMap = ActionMaps.BattleControls,
                menuOption = 1
        });
        dstManager.SetName(entity, "InputManager");
    }
}