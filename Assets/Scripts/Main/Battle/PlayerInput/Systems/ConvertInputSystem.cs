using Reactics.Battle;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertInputSystem : MonoBehaviour, IConvertGameObjectToEntity {
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        dstManager.AddComponentData(entity, new InputData
        {
            currentActionMap = ActionMaps.BattleControls,
            previousActionMap = ActionMaps.BattleControls,
            menuOption = 1
        });
#if UNITY_EDITOR
        dstManager.SetName(entity, "InputManager");
#endif
    }
}