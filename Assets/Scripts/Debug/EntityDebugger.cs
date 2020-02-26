using System;
using Reactics.Battle;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;

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
            EntityManager.SetName(mapEntity, "ImTheMap");
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
            EntityManager.SetName(mapRenderer, "MapRenderer");
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
            EntityManager.SetName(body, "MapBody");

            EntityQuery query = EntityManager.CreateEntityQuery(typeof(CameraMovementData));
            NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);

            //Set some camera component data
            //These only need to be set once so getting the component data at any other point is a waste of time...
            float mapTileSize = 200f; //Hardcoded for now oops. Later we can get it from the map here or in an initialization system or w/e
            EntityManager.SetComponentData(entities[0], new CameraMapData{
                tileSize = mapTileSize, 
                mapLength = mapTileSize * 10,
                mapWidth = mapTileSize * 10
            });

            //Set cursor component data
            var cursor = EntityManager.CreateEntity(typeof(CursorData), typeof(LocalToWorld), typeof(ControlSchemeData), typeof(InitializeTag), typeof(Translation));//GameObjectConversionUtility.ConvertGameObjectHierarchy(cursorGO, World.Active);//EntityManager.CreateEntity(typeof(CursorData), typeof(LocalToWorld), typeof(ControlSchemeData), typeof(InitializeTag), typeof(Translation));
            EntityManager.SetComponentData(cursor, new CursorData
            {
                map = mapEntity,
                cameraEntity = entities[0] //do this but better somehow maybe idk maybe its fine since theres only one camera
            });
            EntityManager.SetName(cursor, "Cursor");
            entities.Dispose();
        }
    }
}