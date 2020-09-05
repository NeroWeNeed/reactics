using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Reactics.Editor {
    public class MapEditor : EditorWindow {
        private const string MAIN_VISUAL_TREE = "Assets/EditorResources/UIElements/MapEditor.uxml";
        private const string TILE_EDITOR_VISUAL_TREE = "Assets/EditorResources/UIElements/MapTileEditor.uxml";
        private const string STYLESHEET = "Assets/EditorResources/UIElements/MapEditor.uss";
        private const string MAP_MATERIAL = "Assets/ResourceData/Materials/MapMaterial.mat";
        public static readonly Color BACKGROUND_COLOR = GeneralCommons.ParseColor("#26541D");
        public static readonly string MAP_EDITOR_TARGET_PREF = "map-editor-target";

        public static MapEditor ShowWindow() {
            var window = GetWindow<MapEditor>();
            window.titleContent = new GUIContent("Map Editor");
            window.minSize = new Vector2(500, 500);
            return window;
        }
        [OnOpenAssetAttribute(1)]
        public static bool OnOpen(int instanceId, int line) {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is MapAsset) {
                ShowWindow().SerializedObject = new SerializedObject(obj);
                return true;
            }
            else {
                return false;
            }

        }
        private static SerializedObject TryLoad(int instanceId) {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is MapAsset) {
                return new SerializedObject(obj);

            }
            else {
                return null;
            }
        }
        private SerializedObject _serializedObject;
        public SerializedObject SerializedObject
        {
            get => _serializedObject; set
            {
                _serializedObject = value;
                rootVisualElement.Unbind();
                if (value != null) {
                    rootVisualElement.Bind(value);
                    var obj = value.targetObject as MapAsset;
                    mapInfo.Mesh = obj.CreateMesh(1, 0.25f, mapInfo.Mesh);
                    Target = value.targetObject as MapAsset;
                    oldWidth = obj.Width;
                    oldLength = obj.Length;
                    EditorPrefs.SetInt(MAP_EDITOR_TARGET_PREF, obj.GetInstanceID());
                    TopDownCamera();
                }
                else {
                    mapInfo.Filter.sharedMesh = null;

                    Target = null;
                }
            }
        }
        public MapAsset Target { get; private set; }
        private CameraInfo cameraInfo;
        private MapInfo mapInfo;

        private SelectedPointInfo selectedPointInfo = SelectedPointInfo.Create();
        private IMGUIContainer cameraContainer;
        private Scene editorScene;
        private List<Point>[] pointBuffers;
        private bool redrawMesh = false;
        private int oldWidth, oldLength;
        private HashSet<Point> selectedPoints = new HashSet<Point>();
        private EditorMapHighlightManager<MapAssetTile, MapAssetSpawnGroup> highlightManager;

        private SelectionManager selectionManager = new SelectionManager();

        private Type selectionBrushType = typeof(OutlineEllipseSelectionBrush);
        private void OnEnable() {

            InitializeScene();
            pointBuffers = new List<Point>[MapLayers.Count];
            for (int i = 0; i < pointBuffers.Length; i++)
                pointBuffers[i] = new List<Point>();
            highlightManager = new EditorMapHighlightManager<MapAssetTile, MapAssetSpawnGroup>();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MAIN_VISUAL_TREE);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLESHEET);
            visualTree.CloneTree(rootVisualElement);
            rootVisualElement.styleSheets.Add(styleSheet);

            cameraContainer = rootVisualElement.Q<IMGUIContainer>("camera-element");
            CreateControls(cameraContainer);
            CreateMeshFieldValidators(rootVisualElement);
            var lastId = EditorPrefs.GetInt(MAP_EDITOR_TARGET_PREF, -1);
            if (lastId > 0) {
                var lastObj = TryLoad(lastId);
                if (lastObj != null)
                    SerializedObject = lastObj;
            }
            selectionManager.context.AddListener((evt) =>
            {
                UpdateTileEditors(evt.current);
                highlightManager.Set(MapLayer.PlayerAll, evt.current);
            });

        }

        private void InitializeScene() {
            editorScene = EditorSceneManager.NewPreviewScene();
            if (cameraInfo.Equals(default))
                cameraInfo = CameraInfo.Create(editorScene);
            else
                SceneManager.MoveGameObjectToScene(cameraInfo.GameObject, editorScene);
            if (mapInfo.Equals(default))
                mapInfo = MapInfo.Create(editorScene, AssetDatabase.LoadAssetAtPath<Material>(MAP_MATERIAL), MapLayers.CreateDefaultColorMap());
            else
                SceneManager.MoveGameObjectToScene(mapInfo.GameObject, editorScene);
        }
        private void OnGUI() {
            if (redrawMesh) {
                mapInfo.Mesh = Target.UpdateMesh(1, 0.25f, oldWidth, oldLength, mapInfo.Mesh);
                redrawMesh = false;
                oldWidth = Target.Width;
                oldLength = Target.Length;
            }
            else
                highlightManager.UpdateMesh(Target, mapInfo.Mesh);

            Handles.DrawCamera(cameraContainer.layout, cameraInfo.Camera);
        }


        private void OnDisable() {
            if (!cameraInfo.Equals(default)) {
                DestroyImmediate(cameraInfo.GameObject);
                cameraInfo = default;
            }
            if (!mapInfo.Equals(default)) {
                DestroyImmediate(mapInfo.GameObject);
                cameraInfo = default;
            }
            EditorSceneManager.ClosePreviewScene(editorScene);
        }

        private void CreateMeshFieldValidators(VisualElement element) {
            RegisterMeshHandlerElement<IntegerField, int>(element, "width-element", (oldValue, newValue) => newValue > 0 && newValue < ushort.MaxValue);
            RegisterMeshHandlerElement<IntegerField, int>(element, "length-element", (oldValue, newValue) => newValue > 0 && newValue < ushort.MaxValue);
        }
        private void RegisterMeshHandlerElement<T, V>(VisualElement parent, string name, Func<V, V, bool> validator) where T : VisualElement, INotifyValueChanged<V>, IBindable {
            var element = parent.Q<T>(name);

            element.RegisterValueChangedCallback((evt) =>
            {
                if (!validator.Invoke(element.value, evt.newValue)) {
                    element.value = element.value;
                    evt.StopImmediatePropagation();
                }
                else {
                    redrawMesh = true;
                }
            });
        }
        private void CreateControls(IMGUIContainer cameraContainer) {
            cameraContainer.RegisterCallback<MouseMoveEvent>((evt) =>
            {
                var dx = evt.mouseDelta.x != 0 ? Math.Sign(evt.mouseDelta.x) : 0;
                var dy = evt.mouseDelta.y != 0 ? Math.Sign(evt.mouseDelta.y) : 0;
                var dirty = false;
                if ((evt.pressedButtons >> 1 & 0b1) == 1) {
                    cameraInfo.GameObject.transform.Rotate(Vector3.up * dx + cameraInfo.Camera.transform.right * dy, Space.World);
                    dirty = true;
                }
                else if ((evt.pressedButtons >> 2 & 0b1) == 1) {
                    cameraInfo.GameObject.transform.position += cameraInfo.Camera.transform.right * -dx + cameraInfo.Camera.transform.up * dy;
                    dirty = true;
                }
                if (TryGetPoint(evt.mousePosition, cameraContainer, out Point point)) {
                    if ((evt.pressedButtons & 1) != 0) {
                        selectionManager.TryUpdateStroke(point);

                    }
                    highlightManager.Set(point, MapLayer.Hover);


                    rootVisualElement.Q<Label>("footer").text = $"X: {point.x}, Y: {point.y}";
                }
                else {

                    highlightManager.Clear(MapLayer.Hover);
                    rootVisualElement.Q<Label>("footer").text = "";
                }

                if (dirty)
                    Repaint();
            });
            //Selecting Tiles
            cameraContainer.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if ((evt.pressedButtons & 1) != 0 && TryGetPoint(evt.mousePosition, cameraContainer, out Point point)) {

                    selectionManager.TryStartStroke(point, selectionBrushType, !evt.ctrlKey);

                }
            });
            cameraContainer.RegisterCallback<MouseUpEvent>((evt) =>
            {
                selectionManager.TryEndStroke();

            });

            cameraContainer.RegisterCallback<WheelEvent>((evt) =>
            {
                if (evt.delta.y != 0) {
                    var d = -Mathf.Sign(evt.delta.y);
                    cameraInfo.Camera.transform.position += cameraInfo.Camera.transform.forward * d;
                    Repaint();
                }

            });
            cameraContainer.Q<Button>("camera-control-center").clicked += CenterCamera;
            cameraContainer.Q<Button>("camera-control-topdown").clicked += TopDownCamera;
        }
        private void UpdateTileEditors(IEnumerable<Point> points) {
            var container = rootVisualElement.Q<Foldout>("tile-properties");
            var editor = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TILE_EDITOR_VISUAL_TREE);
            var existing = new List<Point>();
            for (int i = 0; i < container.childCount; i++) {
                if (container[i].userData is Point point) {
                    if (!points.Contains(point)) {
                        container[i].Unbind();
                        container.RemoveAt(i--);
                    }
                    else
                        existing.Add(point);

                }

            }

            foreach (var point in points) {
                if (existing.Contains(point))
                    continue;
                var e = new BindableElement();
                editor.CloneTree(e);
                ConfigureTileEditor(e, point);
                container.Add(e);

            }
        }
        private void ConfigureTileEditor(BindableElement tileEditor, Point point) {
            tileEditor.userData = point;
            tileEditor.Q<Foldout>().text = $"Tile ({point.x},{point.y})";
            tileEditor.BindProperty(SerializedObject.FindProperty("tiles").GetArrayElementAtIndex(Target.IndexOf(point)));
        }

        private bool TryGetPoint(Vector2 mousePosition, VisualElement element, out Point point) {
            if (Target != null) {
                Vector3 mouseCoordinates = mousePosition - element.worldBound.position;
                mouseCoordinates.y = (element.layout.height - mouseCoordinates.y) / element.layout.height;
                mouseCoordinates.x /= element.layout.width;
                if (mapInfo.Collider.Raycast(cameraInfo.Camera.ViewportPointToRay(mouseCoordinates), out RaycastHit hit, cameraInfo.Camera.farClipPlane)) {
                    if (Target.GetPoint(hit.point, out point, false))
                        return true;
                }
            }
            point = default;
            return false;
        }

        private struct SelectedPointInfo {
            public HashSet<Point> points;




            public static SelectedPointInfo Create() => new SelectedPointInfo
            {
                points = new HashSet<Point>()

            };
            public bool Select(Point point, bool shouldClear) {
                if (shouldClear)
                    points.Clear();
                return points.Add(point);

            }
        }

        #region Game Object Info

        private struct CameraInfo : IEquatable<CameraInfo> {
            public Camera Camera { get; private set; }
            private GameObject gameObject;

            public static CameraInfo Create(Scene scene) {
                var info = new CameraInfo
                {
                    GameObject = new GameObject("Camera", typeof(Camera))
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    }
                };
                SceneManager.MoveGameObjectToScene(info.GameObject, scene);

                info.Camera.clearFlags = CameraClearFlags.SolidColor;
                info.Camera.backgroundColor = BACKGROUND_COLOR;
                info.Camera.scene = scene;
                info.Camera.nearClipPlane = 0.01f;
                info.Camera.farClipPlane = 6000f;

                return info;
            }

            public bool Equals(CameraInfo other) {
                return gameObject == other.GameObject;
            }

            public GameObject GameObject
            {
                get => gameObject; set
                {
                    gameObject = value;
                    if (value == null)
                        Camera = null;
                    else {
                        Camera = gameObject.GetComponent<Camera>();
                    }
                }
            }
        }
        private struct MapInfo : IEquatable<MapInfo> {
            private GameObject gameObject;
            public MeshRenderer Renderer { get; private set; }

            public MeshFilter Filter { get; private set; }

            public MeshCollider Collider { get; private set; }

            public Mesh Mesh
            {
                get => Filter.sharedMesh; set
                {

                    Filter.sharedMesh = value;

                    Collider.sharedMesh = value;


                }
            }

            public GameObject GameObject
            {
                get => gameObject; set
                {
                    gameObject = value;
                    if (value == null) {
                        Renderer = null;
                        Filter = null;
                        Collider = null;
                    }
                    else {
                        Renderer = gameObject.GetComponent<MeshRenderer>();
                        Filter = gameObject.GetComponent<MeshFilter>();
                        Collider = gameObject.GetComponent<MeshCollider>();
                    }
                }
            }

            public static MapInfo Create(Scene scene, Material material, EnumDictionary<MapLayer, Color> layerColors) {
                var info = new MapInfo
                {
                    GameObject = new GameObject("Map", typeof(MeshRenderer), typeof(MeshCollider), typeof(MeshFilter))
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    }
                };
                SceneManager.MoveGameObjectToScene(info.GameObject, scene);


                var materials = new Material[MapLayers.Count];

                for (int i = 0; i < MapLayers.Count; i++) {
                    materials[i] = material;
                }
                info.Renderer.sharedMaterials = materials;




                for (int i = 0; i < MapLayers.Count; i++) {
                    var props = new MaterialPropertyBlock();
                    props.SetColor("_Color", layerColors[(MapLayer)MapLayers.Get(i)]);
                    info.Renderer.SetPropertyBlock(props, i);
                }

                return info;
            }

            public bool Equals(MapInfo other) {
                return gameObject == other.GameObject;
            }
        }
        #endregion

        #region UI Controls
        public void TopDownCamera() {
            if (SerializedObject != null && SerializedObject.targetObject != null) {
                var obj = SerializedObject.targetObject as MapAsset;
                var center = obj.GetCenterInWorldCoordinates();
                cameraInfo.Camera.transform.position = center + ((float3)Vector3.up * (obj.Length * 0.5f / math.tan(math.radians(cameraInfo.Camera.fieldOfView * 0.45f))));

                cameraInfo.Camera.transform.LookAt(center);
                Repaint();
            }
        }
        public void CenterCamera() {
            if (SerializedObject != null && SerializedObject.targetObject != null) {
                var obj = SerializedObject.targetObject as MapAsset;
                var center = obj.GetCenterInWorldCoordinates();
                var dist = obj.GetMaxDistance();
                if (Vector3.Distance(center, cameraInfo.Camera.transform.position) > dist) {
                    cameraInfo.Camera.transform.position = center + ((float3)Vector3.Normalize(Vector3.Project(center, cameraInfo.Camera.transform.position))) * dist;

                }
                if (cameraInfo.Camera.transform.position.y < 0)
                    cameraInfo.Camera.transform.position = new Vector3(cameraInfo.Camera.transform.position.x, cameraInfo.Camera.transform.position.y * -1, cameraInfo.Camera.transform.position.z);
                cameraInfo.Camera.transform.LookAt(center);
                Repaint();
            }
        }
        public void AddTileEditor() {

        }

        #endregion

    }

    /// <summary>
    /// Highlight Manager for editor only.
    /// </summary>
    public class EditorMapHighlightManager<ITile, ISpawnGroup> : IMapHighlightInfo where ITile : IMapTile where ISpawnGroup : IMapSpawnGroup {

        public readonly List<Point>[] pointBuffer = new List<Point>[MapLayers.Count];
        public readonly List<int>[] indexBuffer = new List<int>[MapLayers.Count];
        private readonly List<uint> processedBuffer = new List<uint>();



        public ushort Dirty { get; private set; }

        public EditorMapHighlightManager() {

            for (int i = 0; i < pointBuffer.Length; i++) {
                pointBuffer[i] = new List<Point>();
                indexBuffer[i] = new List<int>();
            }

        }


        public void Set(Point point, params MapLayer[] layers) {


            foreach (var layer in layers) {
                var buffer = pointBuffer[MapLayers.IndexOf(layer)];
                switch (buffer.Count) {
                    case 0:
                        buffer.Add(point);
                        break;
                    case 1:
                        buffer[0] = point;
                        break;
                    default:
                        pointBuffer[MapLayers.IndexOf(layer)].Clear();
                        buffer.Add(point);
                        break;
                }



                Dirty |= (ushort)layer;

            }
        }

        public void Set(MapLayer layer, IEnumerable<Point> points) {



            var buffer = pointBuffer[MapLayers.IndexOf(layer)];
            if (buffer.Count > 0) {
                buffer.Clear();
            }
            buffer.AddRange(points);
            Dirty |= (ushort)layer;


        }
        public void Add(Point point, params MapLayer[] layers) {

            foreach (var layer in layers) {
                pointBuffer[MapLayers.IndexOf(layer)].Add(point);

                Dirty |= (ushort)layer;

            }
        }
        public void Remove(Point point, params MapLayer[] layers) {

            foreach (var layer in layers) {
                pointBuffer[MapLayers.IndexOf(layer)].Remove(point);
                Dirty |= (ushort)layer;
            }
        }
        public void Clear(params MapLayer[] layers) {

            foreach (var layer in layers) {
                if (pointBuffer[MapLayers.IndexOf(layer)].Count > 0)
                    Dirty |= (ushort)layer;
                pointBuffer[MapLayers.IndexOf(layer)].Clear();
            }
        }
        public void ClearAll() {

            for (int i = 0; i < MapLayers.Count; i++) {
                if (pointBuffer[i].Count > 0)
                    Dirty |= (ushort)MapLayers.Get(i);
                pointBuffer[i].Clear();
            }
        }
        public ushort UpdateMesh(IMap<ITile, ISpawnGroup> map, Mesh mesh) {
            if (map == null || mesh == null || Dirty == 0)
                return 0;

            for (byte i = 1; i < MapLayers.Count; i++) {


                indexBuffer[i].Clear();
                if (MapMeshCommons.UpdateRenderLayerBuffer(this, map.Width, MapLayers.Get(i), indexBuffer[i], processedBuffer)) {

                    mesh.SetIndices(indexBuffer[i], MeshTopology.Triangles, i);
                }

            }

            var old = Dirty;


            Dirty = 0;
            return old;
        }

        public IEnumerator<Point> GetPoints(ushort layer) => pointBuffer[MapLayers.IndexOf(layer)].GetEnumerator();

        public IEnumerator<Point> GetPoints(MapLayer layer) => pointBuffer[MapLayers.IndexOf(layer)].GetEnumerator();

    }




}