using System;
using Reactics.Battle;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
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

        [SerializeField]
        private Mesh mesh;
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
            var systems = new Type[] { typeof(MapRenderSystem), typeof(MapBodyPathFindingSystem), typeof(MapBodyMovementSystem),typeof(MapBodyToWorldSystem) };
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(World.DefaultGameObjectInjectionWorld, systems);


            //world.AddSystem(new MapHighlightSystem2(Archetypes));
            DynamicBuffer<HighlightTile> highlights = EntityManager.AddBuffer<HighlightTile>(mapEntity);
            highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(2, 0), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(0, 4), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(6, 6), layer = MapLayer.HOVER });
            highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });

            var body = EntityManager.CreateEntity(typeof(MapBodyTranslation), typeof(MapBody), typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation));
            EntityManager.SetComponentData(body, new MapBody
            {
                point = new Point(0, 0),
                map = mapEntity,
                speed = 4
            });
            EntityManager.SetComponentData(body, new MapBodyTranslation
            {
                point = new Point(4, 5)
            });
            EntityManager.SetSharedComponentData(body, new RenderMesh
            {
                mesh = mesh,
                material = baseMaterial,
                subMesh = 0
            });
        }
    }
}