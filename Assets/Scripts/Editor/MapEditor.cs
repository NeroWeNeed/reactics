using UnityEngine;
using UnityEditor;
using Reactics.Battle;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Reactics.Debugger;
using Reactics.Util;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using Reactics.UIElements;

namespace Reactics.Editors
{

    public class MapEditor : EditorScene
    {
        private MapDebugger debugger;

        [ResourceField("Editor/MapEditor.uxml")]
        private VisualTreeAsset globalPropertiesElement;
        [ResourceField("Editor/MapEditorTileProperties.uxml")]
        private VisualTreeAsset tilePropertiesElement;

        [ResourceField("Editor/MapEditor.uss")]
        private StyleSheet styleSheet;

        private VisualElement pointRequest = null;

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (Selection.activeObject as MapAsset != null)
            {
                GetSceneView<MapEditor>(Selection.activeObject as MapAsset);
                return true;
            }
            return false;
        }
        private void Awake()
        {
            this.InjectResources();
        }

        private void OnEnable()
        {
            this.InjectResources();
        }
        private void OnSceneGUI()
        {
            if (debugger != null)
            {
                Ray ray;
                Point pointInfo;
                RaycastHit hit;
                switch (Event.current.type)
                {
                    case EventType.MouseMove:
                        ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        if (Physics.Raycast(ray, out hit) && debugger.MapRenderer.GetPoint(hit.point, out pointInfo))
                        {
                            debugger.MapRenderer.Hover(pointInfo);
                        }
                        else
                        {
                            debugger.MapRenderer.UnHover();
                        }
                        break;


                    case EventType.MouseDown:

                        ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        if (Physics.Raycast(ray, out hit) && debugger.MapRenderer.GetPoint(hit.point, out pointInfo))
                        {
                            VisualElement tilePropertiesContainerElement = Window.rootElement.Query<VisualElement>("tile-properties-container");
                            if (pointRequest != null)
                            {
                                pointRequest.Q<IntegerField>("x").value = pointInfo.x;
                                pointRequest.Q<IntegerField>("y").value = pointInfo.y;
                                pointRequest = null;
                            }
                            else if (Event.current.modifiers == EventModifiers.Control)
                            {
                                UpdateTilePropertyEditors(tilePropertiesContainerElement, pointInfo, true);
                            }
                            else if (Event.current.clickCount % 2 == 0)
                            {
                                tilePropertiesContainerElement.Clear();
                                UpdateTilePropertyEditors(tilePropertiesContainerElement, pointInfo, false);
                            }
                        }
                        else if (Event.current.clickCount % 2 == 0)
                        {
                            Window.rootElement.Query<VisualElement>("tile-properties-container").ForEach(x => x.Clear());

                        }
                        break;

                }
                if (serializedObject.UpdateIfRequiredOrScript())
                {
                    if (debugger.MapRenderer.UpdateMesh(Target as MapAsset))
                    {
                        Window.rootElement.Query<VisualElement>("tile-properties-container").ForEach(x => x.Clear());
                        serializedObject.FindProperty("_tiles").arraySize = (Target as MapAsset).Width * (Target as MapAsset).Length;

                    }

                }
            }


        }

        private void UpdateTilePropertyEditors(VisualElement container, Point point, bool remove = true)
        {
            BindableElement tileElement = container.Q<BindableElement>($"point-{point.x}-{point.y}");
            if (tileElement == null)
            {
                tileElement = new BindableElement
                {
                    name = $"point-{point.x}-{point.y}"
                };
                tilePropertiesElement.CloneTree(tileElement);
                tileElement.Q<Foldout>("header").text = $"X: {point.x}, Y: {point.y}";
                tileElement.BindProperty(serializedObject.FindProperty("_tiles").GetArrayElementAtIndex(debugger.MapRenderer.Map.IndexOf(point)));
                container.Add(tileElement);

            }
            else if (remove)
            {
                tileElement.RemoveFromHierarchy();
            }
        }

        private void OnSceneInit(Scene scene)
        {
            this.InjectResources();
            new GameObject("Map Debugger").Apply(obj =>
            {
                debugger = obj.AddComponent<MapDebugger>();
                debugger.MapRenderer.Map = Target as MapAsset;
                debugger.MapRenderer.FocusCamera(Scene.SceneView.camera);
                float maxDistance = debugger.MapRenderer.GetMaxCameraDistance();
                Vector3 center = debugger.MapRenderer.GetCenter();
                if (Vector3.Distance(center, Scene.SceneView.camera.transform.position) > maxDistance)
                    Scene.SceneView.pivot = -(Scene.SceneView.camera.transform.position - center).normalized * maxDistance;
                if (Scene.SceneView.pivot.y < center.y + maxDistance * 0.5f)
                    Scene.SceneView.pivot = new Vector3(Scene.SceneView.pivot.x, center.y + (maxDistance * 0.5f), Scene.SceneView.pivot.z);
                Scene.SceneView.LookAt(center);
            });
        }

        private VisualElement OnCreateWindowInspector(VisualElement element)
        {
            element.Clear();
            globalPropertiesElement.CloneTree(element);
            element.styleSheets.Clear();
            element.styleSheets.Add(styleSheet);
            element.Q<Button>("new-spawn-group").clicked += () =>
            {
                AddPointList(element);
            };
            foreach (var item in (Target as MapAsset).spawnGroups)
            {
                AddPointList(element, item.points);
            }
            element.Q<VisualElement>("spawn-properties-container").RegisterCallback<ChangeEvent<Point[]>>(x =>
        {

            ReserlializeSpawnGroups(element);
        });
            return element;
        }
        private void ReserlializeSpawnGroups(VisualElement root)
        {
            int elementIndex = 0;
            int pointIndex;
            SerializedProperty spawnGroupsProperty = serializedObject.FindProperty("_spawnGroups");
            SerializedProperty property, pointProperty;
            spawnGroupsProperty.ClearArray();
            root.Q<VisualElement>("spawn-properties-container").Query<PointListElement>().ForEach(y =>
            {
                spawnGroupsProperty.InsertArrayElementAtIndex(elementIndex);
                property = spawnGroupsProperty.GetArrayElementAtIndex(elementIndex).FindPropertyRelative("points");
                pointIndex = 0;
                property.ClearArray();
                foreach (var item in y.value)
                {
                    property.InsertArrayElementAtIndex(pointIndex);
                    pointProperty = property.GetArrayElementAtIndex(pointIndex);
                    pointProperty.FindPropertyRelative("_x").intValue = item.x;
                    pointProperty.FindPropertyRelative("_y").intValue = item.y;
                    pointIndex++;
                }
                property.arraySize = pointIndex;
                y.BindProperty(property);
                elementIndex++;
            });
            serializedObject.FindProperty("_spawnGroups").arraySize = elementIndex;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }

        private void AddPointList(VisualElement root, params Point[] points)
        {
            Foldout foldout = new Foldout
            {
                text = "Spawn Group"
            };
            PointListElement pointListElement;
            if (points.Length <= 0)
                pointListElement = new PointListElement(7);
            else
                pointListElement = new PointListElement(points);
            foldout.Add(pointListElement);
            foldout.AddToClassList("targetEditorSubSectionHeader");
            VisualElement align = new VisualElement();
            align.style.flexDirection = FlexDirection.RowReverse;
            align.style.flexGrow = new StyleFloat(1f);

            Button close = new Button
            {
                name = "unity-checkmark",
                text = "✕"
            };
            close.ClearClassList();
            align.Add(close);
            close.AddToClassList("ui-button");
            foldout.Q<VisualElement>("unity-checkmark").parent.Add(align);
            root.Q<VisualElement>("spawn-properties-container").Add(foldout);
            close.clicked += () =>
            {
                foldout.RemoveFromHierarchy();
                ReserlializeSpawnGroups(root);
            };
            ReserlializeSpawnGroups(root);


        }



    }



}
