using Reactics.Battle;
using Unity.Entities;
using UnityEngine;
namespace Reactics.Util
{


    public class EntityDebugger : MonoBehaviour
    {

        [SerializeField]
        private Map map;
        private void Start()
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity mapEntity = map.CreateEntity(entityManager);
#if UNITY_EDITOR
            entityManager.SetName(mapEntity, "Map Entity");
#endif
            DynamicBuffer<HighlightTile> highlights = entityManager.AddBuffer<HighlightTile>(mapEntity);
            highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });
        }
    }
}