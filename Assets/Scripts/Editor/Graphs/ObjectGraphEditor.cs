using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {

    public abstract class ObjectGraphEditor<TAsset> : EditorWindow where TAsset : ScriptableObject {

        protected const string TOOLBAR_UXML = "545591ee989f29744a0e2cc64fa5c84c";

        protected static TEditorWindow ShowWindow<TEditorWindow>(string title) where TEditorWindow : EditorWindow {
            var window = GetWindow<TEditorWindow>();
            window.titleContent = new GUIContent(title);
            window.minSize = new Vector2(640, 480);
            window.Show();
            return window;
        }


        protected static bool OnOpen<TEditorWindow>(int instanceId, int line, Func<TAsset, string> titleFunction) where TEditorWindow : ObjectGraphEditor<TAsset> {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is TAsset asset) {
                var window = ShowWindow<TEditorWindow>(titleFunction.Invoke(asset));
                window.InstanceId = instanceId;
                return true;
            }
            else {
                return false;
            }
        }


        public ObjectGraphView graphView { get; private set; }


        public ObjectGraphModelEditor ModelEditor { get; protected set; }

        public ObjectGraphModel Model { get; protected set; }

        public IObjectGraphModule[] Modules { get; protected set; }






        protected string EditorPrefKey { get => $"{this.GetType().FullName}.Target"; }

        private int instanceId;
        public int InstanceId
        {
            get => instanceId; set
            {
                if (value != instanceId) {
                    instanceId = value;
                    EditorPrefs.SetInt(EditorPrefKey, value);

                    Load(instanceId);
                }
            }
        }

        protected abstract string SaveFileInPanelTitle { get; }
        protected abstract string SaveFileInPanelDefaultName { get; }
        protected abstract string SaveFileInPanelPath { get; }

        protected ObjectGraphEditor(params IObjectGraphModule[] modules) {
            this.Modules = modules;
        }
        public virtual ObjectGraphModel CreateModel() => ScriptableObject.CreateInstance<ObjectGraphModel>();
        public virtual ObjectGraphModelEditor CreateModelEditor(ObjectGraphModel model) => new ObjectGraphModelEditor(model);

        private void OnEnable() {
            if (Model == null) {
                Model = CreateModel();
            }
            ModelEditor = CreateModelEditor(Model);



            PreInit();
            graphView = CreateGraphView();
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(TOOLBAR_UXML)).CloneTree(rootVisualElement);
            this.ConfigureToolbarButtons(rootVisualElement.Q<Toolbar>());
            rootVisualElement.Add(graphView);
            PostInit();

            if (this.InstanceId != 0)
                Load(InstanceId);


        }

        private ObjectGraphView CreateGraphView() {
            var graphView = new ObjectGraphView(ModelEditor, (p) => p - this.position.position, Modules);
            graphView.RegisterCallback<ObjectGraphValidateEvent>((evt) => rootVisualElement.Query<VisualElement>(null, "require-validation").ForEach((e) => e.SetEnabled(evt.isValid)));
            graphView.style.flexGrow = 1;
            return graphView;

        }
        public virtual void PreInit() { }

        public virtual void PostInit() { }

        private void ForceLoad(int instanceId) {
            this.instanceId = instanceId;
            EditorPrefs.SetInt(EditorPrefKey, instanceId);
            Load(instanceId);
        }




        #region Toolbar Actions
        public virtual void Load(int instanceId) {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is TAsset) {
                Load(new SerializedObject(obj));
            }
        }
        public virtual void Load(SerializedObject obj) {
            graphView.Clean();
            graphView.ModelEditor = ModelEditor;
            DoLoad(obj);
            graphView.RefreshInspector(obj);
        }
        protected virtual void DoLoad(SerializedObject obj) {
            foreach (var module in graphView.Modules) {
                if (module is IObjectGraphNodeProvider provider && !provider.Deserialize(obj, graphView)) {
                    throw new UnityException("Error serializing Asset");
                }
            }


        }
        [ToolbarAction("save")]
        public virtual void Save() => Save(InstanceId);
        public virtual void Save(int instanceId) {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is TAsset) {
                Save(new SerializedObject(obj));
            }
            else {
                Debug.LogError("Failed to Save.");
            }
        }

        [ToolbarAction("save-as")]
        public virtual void SaveAs() {
            var path = EditorUtility.SaveFilePanelInProject(SaveFileInPanelTitle, SaveFileInPanelDefaultName, "asset", null, SaveFileInPanelPath);
            if (!string.IsNullOrEmpty(path)) {
                var asset = CreateInstance<TAsset>();
                AssetDatabase.CreateAsset(asset, path);
                EditorUtility.SetDirty(asset);
                Save(new SerializedObject(asset));
            }
        }
        public virtual void Save(SerializedObject obj) {
            DoSave(obj);
            AssetDatabase.SaveAssets();
        }
        protected virtual void DoSave(SerializedObject obj) {
            foreach (var module in graphView.Modules) {
                if (module is IObjectGraphNodeProvider provider && !provider.Serialize(obj, graphView)) {
                    throw new UnityException("Error serializing Asset");
                }
            }
        }
        [ToolbarAction("show-in-project")]
        protected virtual void ShowInProject() {
            if (instanceId != 0) {
                ProjectWindowUtil.ShowCreatedAsset(EditorUtility.InstanceIDToObject(instanceId));
            }
        }
        #endregion
    }
}