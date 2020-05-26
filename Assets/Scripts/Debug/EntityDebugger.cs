using System;
using Reactics.Battle;
using Reactics.UI;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using Unity.Mathematics;

namespace Reactics.Util
{

    //I had some changes here but this file exploded during a merge conflict so I'm just using yours for now since I'll probably have to change stuff with the map blobs anyway
    //also this is just a debugger so seeing those changes doesn't *really* matter much
    public class EntityDebugger : MonoBehaviour
    {
        [SerializeField]
        private Map map;
        [SerializeField]
        private Unit testUnit;
        [SerializeField]
        private Unit testUnit2;
        [ResourceField("Materials/Map/HoverMaterial.mat")]
        private Material hoverMaterial;
        [ResourceField("Materials/Map/MapMaterial.mat")]
        private Material baseMaterial;

        [SerializeField]
        private Mesh mesh;

        [SerializeField]
        private Mesh uiMesh;

        [SerializeField]
        private TMP_FontAsset fontAsset;
        private void Awake()
        {
            this.InjectResources();


            var simSystems = new Type[] { typeof(MapSystemGroup), typeof(MapRenderSystemGroup), typeof(MapRenderSystem), typeof(MapLayerRenderSystem), typeof(MapBodyPathFindingSystem) };

            //EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            World world = World.DefaultGameObjectInjectionWorld;
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BattleSimulationSystemGroup>();
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BattleSimulationEntityCommandBufferSystem>();

            //SimulationWorld simulationWorld = new SimulationWorld("Sample Simulation", simSystems);

            EditorApplication.playModeStateChanged += Cleanup;
            EntityManager EntityManager = world.EntityManager;
            var mapEntity = EntityManager.CreateEntity(typeof(MapData), typeof(MapRenderData), typeof(MapTileEffect));

            EntityManager.SetComponentData(mapEntity, map.CreateComponent());
            float mapTileSize = 1f;
            EntityManager.SetComponentData(mapEntity,new MapRenderData {
                tileSize = mapTileSize,
                elevationStep = 0.25f
            });
            var renderMap = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            var renderMap2 = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            var renderMap3 = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            var renderMap4 = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            var renderMap5 = EntityManager.CreateEntity(typeof(RenderMap), typeof(Translation), typeof(LocalToWorld));
            var inputEntity = EntityManager.CreateEntity(typeof(InputData));
            var unitManagerEntity = EntityManager.CreateEntity(typeof(UnitManagerData));
            EntityManager.SetComponentData(renderMap, new RenderMap
            {
                layer = MapLayer.BASE
            });
            EntityManager.SetComponentData(renderMap2, new RenderMap
            {
                layer = MapLayer.HOVER
            });
            EntityManager.SetComponentData(renderMap3, new RenderMap
            {
                layer = MapLayer.PLAYER_MOVE
            });
            EntityManager.SetComponentData(renderMap4, new RenderMap
            {
                layer = MapLayer.PLAYER_ATTACK
            });
            EntityManager.SetComponentData(renderMap4, new RenderMap
            {
                layer = MapLayer.PLAYER_SUPPORT
            });

            EntityManager.SetName(inputEntity, "InputEntity");
            EntityManager.SetComponentData(inputEntity, new InputData
            {
                currentActionMap = ActionMaps.BattleControls,
                previousActionMap = ActionMaps.BattleControls,
                menuOption = 1
            });
            EntityManager.SetName(unitManagerEntity, "UnitManager");
            EntityManager.SetComponentData(unitManagerEntity, new UnitManagerData
            {
                commanding = false
            });

            //Create a map body unit
            var mapBody = EntityManager.CreateEntity(typeof(MapBodyTranslation), typeof(MapBody), typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation), 
                typeof(UnitData), typeof(ActionMeter), typeof(UnitCommand), typeof(MoveTilesTag), typeof(HighlightTile));
            var mapBodyUnitData = EntityManager.GetComponentData<UnitData>(mapBody);
            EntityManager.SetComponentData(mapBody, new MapBody
            {
                point = new Point(6, 9),
                speed = 4,
                self = mapBody
            });
            /*EntityManager.SetComponentData(mapBody, new MapBodyTranslation
            {
                point = new Point(4, 5)
            });*/
            EntityManager.SetSharedComponentData(mapBody, new RenderMesh
            {
                mesh = mesh,
                material = baseMaterial,
                subMesh = 0
            });
            EntityManager.SetComponentData(mapBody, new ActionMeter
            {
                rechargeRate = 10f,
                chargeable = true,
                charge = 0f
            });
            EntityManager.SetComponentData(mapBody, new UnitCommand
            {
                unitManagerEntity = unitManagerEntity
            });
            EntityManager.SetComponentData(mapBody, testUnit.CreateComponent());
            DynamicBuffer<EffectBuffer> effectBuffer = EntityManager.AddBuffer<EffectBuffer>(mapBody);
            EntityManager.SetName(mapBody, "Galvinius");

            //Create a map body unit
            var body2 = EntityManager.CreateEntity(typeof(MapBodyTranslation), typeof(MapBody), typeof(RenderMesh), typeof(LocalToWorld), typeof(Translation), 
                typeof(UnitData), typeof(ActionMeter), typeof(UnitCommand), typeof(MoveTilesTag), typeof(HighlightTile));
            EntityManager.SetComponentData(body2, new MapBody
            {
                point = new Point(6, 6),
                speed = 4,
                self = body2
            });
            /*EntityManager.SetComponentData(body2, new MapBodyTranslation
            {
                point = new Point(4, 9)
            });*/
            EntityManager.SetSharedComponentData(body2, new RenderMesh
            {
                mesh = mesh,
                material = baseMaterial,
                subMesh = 0
            });
            EntityManager.SetComponentData(body2, new ActionMeter
            {
                rechargeRate = 20f,
                chargeable = true,
                charge = 100f
            });
            EntityManager.SetComponentData(body2, new UnitCommand
            {
                unitManagerEntity = unitManagerEntity
            });
            EntityManager.SetComponentData(body2, testUnit2.CreateComponent());
            DynamicBuffer<EffectBuffer> effectBuffer2 = EntityManager.AddBuffer<EffectBuffer>(body2);
            EntityManager.SetName(body2, "Talfias");
            /*
            for (int j = 0; j < 2; j++)
                for (int i = 0; i < 4; i++)
                {
                    var body = EntityManager.CreateEntity(typeof(MapBodyTranslation), typeof(MapBody), typeof(RenderMesh), typeof(LocalToWorld), typeof(MapBodyMeshOffset));
                    EntityManager.SetComponentData(body, new MapBody
                    {
                        point = new Point(i % map.Width, j),
                        speed = 4,
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
            var uiEntity = EntityManager.CreateEntity(typeof(LocalToScreen), typeof(RenderMesh), typeof(LocalToWorld), typeof(EntityGuid));
            EntityManager.SetComponentData(uiEntity, new LocalToScreen
            {
                location = new Unity.Mathematics.float2(100, -100),
                screenAnchor = UIAnchor.TOP_LEFT
            });
            EntityManager.SetSharedComponentData(uiEntity, new RenderMesh
            {
                mesh = uiMesh,
                material = baseMaterial,
                subMesh = 0,
                layer = 5

            });
            var textEntity = EntityManager.CreateEntity(UIArchetypes.DirtyUIElement);
            EntityManager.SetComponentData(textEntity, new LocalToScreen
            {
                location = new Unity.Mathematics.float2(-100, -100),
                screenAnchor = UIAnchor.TOP_RIGHT,
                localAnchor = UIAnchor.TOP_RIGHT
                
            });
            EntityManager.SetSharedComponentData(textEntity, new Reactics.UI.UIElement
            {
                configurator = new TextMeshFactory()
            });
            EntityManager.AddComponent<UIText>(textEntity);
            EntityManager.SetSharedComponentData(textEntity, new UIText
            {
                value = "Inject"
            });
            
            EntityManager.AddSharedComponentData(textEntity,new UIFont
            {
                value = fontAsset
            });
            EntityManager.AddComponentData(textEntity,new UITextSettings
            {
                fontSize = 12
            });
            

            /*             var textEntity = EntityManager.CreateEntity(UIArchetypes.DirtyUIElement);
                        EntityManager.SetComponentData(textEntity, new LocalToScreen
                        {
                            location = new Unity.Mathematics.float2(-100, -100),
                            screenAnchor = UIAnchor.TOP_RIGHT
                        });
                        EntityManager.AddSharedComponentData(textEntity, new UIText
                        {
                            text = "Testing",
                            fontAsset = fontAsset
                        }); */
            /*             EntityManager.SetSharedComponentData(textEntity,new Reactics.UI.UIElement {
                            configurator = new TextMeshFactory()
                        });
                        Debug.Log(EntityManager.GetSharedComponentData<Reactics.UI.UIElement>(textEntity).configurator); */

            //world.GetExistingSystem<BattleSimulationSystemGroup>().UpdateCallback = (x) => false;
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