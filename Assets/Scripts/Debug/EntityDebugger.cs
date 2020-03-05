using System;
using Reactics.Battle;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Reactics.Util
{

    //I had some changes here but this file exploded during a merge conflict so I'm just using yours for now since I'll probably have to change stuff with the map blobs anyway
    //also this is just a debugger so seeing those changes doesn't *really* matter much
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


            var simSystems = new Type[] { typeof(MapSystemGroup), typeof(MapRenderSystemGroup), typeof(MapRenderSystem), typeof(MapLayerRenderSystem), typeof(MapBodyPathFindingSystem) };


            World world = World.DefaultGameObjectInjectionWorld;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BattleSimulationSystemGroup>();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BattleSimulationEntityCommandBufferSystem>();

            //SimulationWorld simulationWorld = new SimulationWorld("Sample Simulation", simSystems);

            EditorApplication.playModeStateChanged += Cleanup;
            EntityManager EntityManager = world.EntityManager;
            var mapEntity = EntityManager.CreateEntity(typeof(MapData), typeof(MapRenderData));

            EntityManager.SetComponentData(mapEntity, map.CreateComponent());
            EntityManager.SetComponentData(mapEntity, new MapRenderData
            {
                tileSize = 1f,
                elevationStep = 0.25f
            });
            var renderMap = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            var renderMap2 = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            var otherHighlightEntity = EntityManager.CreateEntity(typeof(HighlightTile));
            EntityManager.SetComponentData(renderMap, new RenderMap
            {
                layer = MapLayer.BASE
            });
            EntityManager.SetComponentData(renderMap2, new RenderMap
            {
                layer = MapLayer.HOVER
            });
            
            for (int i = 0; i < 10; i++)
            {
            var body = EntityManager.CreateEntity(typeof(MapBodyTranslation), typeof(MapBody), typeof(RenderMesh), typeof(LocalToWorld), typeof(MapBodyMeshOffset));
                EntityManager.SetComponentData(body, new MapBody
                {
                    point = new Point(i % map.Width, 0),
                    speed = 8,
                    solid = true
                });
                EntityManager.SetComponentData(body, new MapBodyMeshOffset
                {
                    anchor = MapBodyAnchor.BOTTOM_CENTER
                });
                EntityManager.SetSharedComponentData(body, new RenderMesh
                {
                    mesh = mesh,
                    material = baseMaterial,
                    subMesh = 0
                });
            }

            /*             var renderMap = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
                        var renderMap2 = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
                        var otherHighlightEntity = EntityManager.CreateEntity(typeof(HighlightTile));
                        EntityManager.SetComponentData(renderMap, new RenderMap
                        {
                            layer = MapLayer.BASE
                        });
                        EntityManager.SetComponentData(renderMap2, new RenderMap
                        {
                            layer = MapLayer.HOVER
                        });
                        DynamicBuffer<HighlightTile> highlights = EntityManager.AddBuffer<HighlightTile>(otherHighlightEntity);
                        highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });
                        highlights.Add(new HighlightTile { point = new Point(2, 0), layer = MapLayer.HOVER });
                        highlights.Add(new HighlightTile { point = new Point(0, 4), layer = MapLayer.HOVER });
                        highlights.Add(new HighlightTile { point = new Point(6, 6), layer = MapLayer.HOVER });
                        highlights.Add(new HighlightTile { point = new Point(0, 0), layer = MapLayer.HOVER });

                        var body = EntityManager.CreateEntity(typeof(MapBodyTranslation), typeof(MapBody), typeof(RenderMesh), typeof(LocalToWorld),typeof(MapBodyMeshOffset));
                        EntityManager.SetComponentData(body, new MapBody
                        {
                            point = new Point(4, 5),
                            speed = 4
                        });
                        EntityManager.SetComponentData(body, new MapBodyMeshOffset {
                            anchor = MapBodyAnchor.BOTTOM_CENTER
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
                        }); */


            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, simSystems);
            /* 
                        
                        //World.DefaultGameObjectInjectionWorld.GetExistingSystem<ExternalSimulationSystem>().SimulationWorld = simulationWorld;
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
             */
        }
        private int previous = -1;
        private void Cleanup(PlayModeStateChange state)
        {
            if (previous == -1)
                previous = (int)state;
            else if ((int)state != previous)
            {
                World.DisposeAllWorlds();
                WordStorage.Instance.Dispose();
                WordStorage.Instance = null;
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop(null);
                previous = (int)state;
            }

        }
    }
}