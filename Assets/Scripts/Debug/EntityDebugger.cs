using System;
using Reactics.Battle;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
namespace Reactics.Util
{


    public class EntityDebugger : MonoBehaviour
    {

        [SerializeField]
        private Map map;
        [ResourceField("Materials/Map/HoverMaterial.mat")]
        private Material hoverMaterial;
        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material baseMaterial;
        private void Start()
        {
            this.InjectResources();


            EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            MapWorld.WorldArchetypes Archetypes = new MapWorld.WorldArchetypes(EntityManager);
            var mapEntity = map.CreateEntity(EntityManager);
            var mapRenderer = EntityManager.CreateEntity(Archetypes.MapRenderer);
            EntityManager.SetSharedComponentData(mapRenderer, new RenderMesh
            {
                mesh = map.GenerateMesh(),
                material = baseMaterial,
                subMesh = 0
            });
            EntityManager.SetComponentData(mapRenderer, new RenderMap
            {
                map = mapEntity

            });
            var systems = new Type[] { typeof(MapRenderSystem) };
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World.DefaultGameObjectInjectionWorld, systems);


            //world.AddSystem(new MapHighlightSystem2(Archetypes));
            DynamicBuffer<HighlightTile> highlights = EntityManager.AddBuffer<HighlightTile>(mapEntity);
            highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(2, 0), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(0, 4), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(6, 6), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });
        }
    }
}