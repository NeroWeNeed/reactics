using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {


    public abstract class BaseBehaviourGraphEditor : EditorWindow {
        public abstract IReadOnlyCollection<BehaviourGraphModel> Models { get; protected set; }
        public abstract ScriptableObject TargetObject { get; set; }
        protected static TWindow ShowWindow<TWindow>(string title) where TWindow : BaseBehaviourGraphEditor {
            var window = GetWindow<TWindow>();
            window.titleContent = new GUIContent(title);
            window.minSize = new Vector2(640, 480);
            window.Show();
            return window;
        }


        public static bool OnOpenIgnoreType<TWindow>(int instanceId, int line = 0) where TWindow : BaseBehaviourGraphEditor {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is ScriptableObject model) {
                var window = ShowWindow<TWindow>(obj.name);
                window.TargetObject = model;
                return true;
            }
            else {
                return false;
            }
        }
        [ToolbarItem("show-in-project", "Show In Project")]
        private void OnShowInProject() {
            if (TargetObject != null)
                EditorGUIUtility.PingObject(TargetObject);
        }



    }
    public abstract class BaseBehaviourGraphEditor<TBehaviourGraphView> : BaseBehaviourGraphEditor where TBehaviourGraphView : BehaviourGraphView {
        protected TBehaviourGraphView graphView;
        public TBehaviourGraphView GraphView
        {
            get => graphView; protected set
            {
                if (graphView != null) {
                    graphView.Dispose();
                    rootVisualElement.Remove(graphView);
                }

                graphView = value;
                rootVisualElement.Add(graphView);
            }
        }
        private ScriptableObject targetObject;
        private readonly List<BehaviourGraphModel> models = new List<BehaviourGraphModel>();
        public override IReadOnlyCollection<BehaviourGraphModel> Models { get; protected set; }
        public override ScriptableObject TargetObject
        {
            get => targetObject; set
            {
                if (targetObject?.GetInstanceID() == value?.GetInstanceID())
                    return;

                this.targetObject = value;
                this.models.Clear();
                if (this.targetObject is BehaviourGraphModel model) {
                    this.models.Add(model);
                }
                else {
                    this.models.AddRange(this.targetObject.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Where(field => typeof(BehaviourGraphModel).IsAssignableFrom(field.FieldType) && field.GetCustomAttribute<EmbedInBehaviourGraphAttribute>() != null).Select(field => field.GetValue(this.targetObject) as BehaviourGraphModel));
                }
                this.Models = models.AsReadOnly();
                //this.Settings = BehaviourGraphGlobalSettings.GetBehaviourGraphSettings(TargetObject?.BehaviourType.Value);
                GraphView = CreateGraphView();
                GraphView.Init(models, this);

                if (TargetObject != null) {

                    EditorPrefs.SetString(nameof(BehaviourGraphEditor), AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(TargetObject)));
                }
                else {
                    EditorPrefs.DeleteKey(nameof(BehaviourGraphEditor));
                }
            }
        }
        private void OnEnable() {

            rootVisualElement.Add(ToolbarUtility.CreateFlatToolbar(this));
            GraphView = CreateGraphView();
            //GraphView.Init(TargetObject, this);
            var lastLoadedId = EditorPrefs.GetString(nameof(BehaviourGraphEditor), null);
            if (!string.IsNullOrEmpty(lastLoadedId)) {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(lastLoadedId));
                if (asset != null)
                    TargetObject = asset;
            }
        }
        private void OnDisable() {
            GraphView?.Dispose();
            if (Models != null) {
                foreach (var model in Models) {
                    model.Clean();
                }
            }

            GraphView = null;
        }
        protected abstract TBehaviourGraphView CreateGraphView();
    }
    public class BehaviourGraphEditor : BaseBehaviourGraphEditor<BehaviourGraphView> {
        [OnOpenAsset(10)]
        public static bool OnOpen(int instanceId, int line = 0) {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is BehaviourGraphModel model) {
                var window = ShowWindow<BehaviourGraphEditor>(obj.name);
                window.TargetObject = model;
                return true;
            }
            else {
                return false;
            }
        }
        [ToolbarItem("show-in-project", "Show In Project")]
        private void OnShowInProject() {
            if (TargetObject != null)
                EditorGUIUtility.PingObject(TargetObject);
        }
        private void OnEnable() {

            rootVisualElement.Add(ToolbarUtility.CreateFlatToolbar(this));
            GraphView = CreateGraphView();
            //GraphView.Init(TargetObject, this);
            var lastLoadedId = EditorPrefs.GetString(nameof(BehaviourGraphEditor), null);
            if (!string.IsNullOrEmpty(lastLoadedId)) {
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(lastLoadedId));
                if (asset != null)
                    TargetObject = asset;
            }
        }
        private void OnDisable() {
            GraphView?.Dispose();
            foreach (var model in Models) {
                model.Clean();
            }
            GraphView = null;
        }

        protected override BehaviourGraphView CreateGraphView() {
            var graphView = new BehaviourGraphView(this);
            graphView.style.backgroundColor = new Color(0.125f, 0.125f, 0.125f, 1.0f);
            graphView.style.flexGrow = 1;
            return graphView;
        }
    }

}