using Unity.Entities;
using UnityEngine;
namespace Reactics.UI.Authoring
{
    [RequiresEntityConversion]
    
    [ConverterVersion("Nero", 1)]
    public class UIEnvironment : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<UIEnvironmentData>(entity);
            
        }
    }
}