namespace Reactics.Util
{
    using Reactics.Battle;
    using Unity.Entities;
    using UnityEngine;

    public class EntityDebugger : MonoBehaviour
    {

        [SerializeField]
        private MapAsset map;
        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity mapEntity = map.CreateEntity(entityManager);
#if UNITY_EDITOR
            entityManager.SetName(mapEntity, "Map Entity");
#endif
        }
    }
}