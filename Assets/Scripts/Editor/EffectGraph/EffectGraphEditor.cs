using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.IO;
using Reactics.Battle;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;
using System;
using UnityEditor.UIElements;

namespace Reactics.Editor
{

    public class EffectGraphEditor : EditorWindow
    {

        public static readonly Vector2 DEFAULT_NODE_SIZE = new Vector2(100, 100);
        public const string DEFAULT_EFFECT_NAME = "EffectAsset";
        public static readonly string EDITOR_PREF_KEY = $"{typeof(EffectGraphEditor).FullName}.Target";

        [MenuItem("Assets/Create/Reactics/Effect")]
        public static void CreateEffectAsset()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string file;
            int tries = 1;
            do
            {
                if (tries > 1)
                    file = path + "/" + DEFAULT_EFFECT_NAME + " (" + tries + ")" + ".asset";
                else
                    file = path + "/" + DEFAULT_EFFECT_NAME + ".asset";
                tries++;
            } while (File.Exists(file));
            var effect = CreateInstance<EffectAsset>();
            var layout = CreateInstance<GraphNodeLayout>();
            layout.name = "Layout";
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(effect.GetInstanceID(), DoCreateEffect.CreateInstance(layout.GetInstanceID()), file, AssetPreview.GetMiniThumbnail(effect), null);

        }

        private static EffectGraphEditor ShowWindow(string title)
        {
            var window = GetWindow<EffectGraphEditor>();
            window.titleContent = new GUIContent(title);
            window.Show();
            return window;
        }

        [OnOpenAsset(1)]
        public static bool OnOpen(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is EffectAsset)
            {
                var window = ShowWindow($"Effect Graph ({obj.name})");
                window.InstanceId = instanceId;
                return true;
            }
            else
                return false;
        }
        public EffectGraphView graphView { get; private set; }
        private EffectGraph effectGraph;
        private int instanceId;
        public int InstanceId
        {
            get => instanceId; set
            {
                if (value != instanceId)
                {
                    instanceId = value;
                    EditorPrefs.SetInt(EDITOR_PREF_KEY, value);
                    Load(instanceId);

                }
            }
        }
        private void OnEnable()
        {
            effectGraph = CreateInstance<EffectGraph>();
            if (graphView == null)
                graphView = new EffectGraphView(effectGraph, (p) => p - this.position.position) { name = "Effect Graph View" };

            AddMasterNode();
            rootVisualElement.Add(CreateToolbar());
            rootVisualElement.Add(graphView);
            graphView.style.flexGrow = 1;
            if (EditorPrefs.HasKey(EDITOR_PREF_KEY))
            {
                var obj = EditorPrefs.GetInt(EDITOR_PREF_KEY);
                if (obj != 0)
                    ForceLoad(obj);
                else
                    EditorPrefs.DeleteKey(EDITOR_PREF_KEY);
            }
        }
        private void AddMasterNode()
        {
            var node = new EffectMasterNode();
            node.AddToClassList(EffectGraphController.EffectGraphMasterNodeClassName);
            node.viewDataKey = EffectGraph.MasterNodeId.ToString();
            graphView.AddElement(node);
        }
        private void ForceLoad(int instanceId)
        {
            this.instanceId = instanceId;
            Load(instanceId);
        }

        private Toolbar CreateToolbar()
        {
            var toolbar = new Toolbar();
            toolbar.Add(
            new ToolbarButton(() => Save(InstanceId))
            {
                text = "Save"
            }
            );
            toolbar.Add(new ToolbarButton(() => SaveAs())
            {
                text = "Save As..."
            });
            toolbar.Add(new ToolbarButton(() => ShowInProject())
            {
                text = "Show In Project"
            });

            return toolbar;
        }
        #region Toolbar Actions
        public void Load(int instanceId)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            Debug.Log(obj.GetType());
            if (obj is EffectAsset)
            {
                var path = AssetDatabase.GetAssetPath(instanceId);
                var layout = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault((x) => x is GraphNodeLayout);
                if (layout != null)
                {
                    Load(new SerializedObject(obj), new SerializedObject(layout));
                }

            }

        }
        public void Load(SerializedObject effectSerializedObject, SerializedObject layoutSerializedObject)
        {
            effectGraph.entries.Clear();
            graphView.Query<VisualElement>(null, EffectGraphController.EffectGraphNodeClassName).ForEach((x) => x.RemoveFromHierarchy());
            EffectGraphController.DeserializeFrom(effectGraph, graphView, effectSerializedObject, layoutSerializedObject, ConnectToMaster);
            
        }
        public void Save(int instanceId)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            if (obj is EffectAsset effect)
            {
                var path = AssetDatabase.GetAssetPath(instanceId);
                var layout = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault((x) => x is GraphNodeLayout) as GraphNodeLayout;
                if (layout != null)
                {
                    var initial = graphView.Q<EffectMasterNode>(null, EffectGraphController.EffectGraphMasterNodeClassName).EffectPort.connections.Select((x) => x.output.node).ToArray();
                    EffectGraphController.SerializeTo(effectGraph, effect, layout, initial, new NodeReader());

                }

            }
        }
        public void SaveAs()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Effect As...", "EffectAsset", "asset", null, "Assets/ResourceData/Effects");
            if (!string.IsNullOrEmpty(path))
            {
                var effectAsset = CreateInstance<EffectAsset>();
                var layoutAsset = CreateInstance<GraphNodeLayout>();
                AssetDatabase.CreateAsset(effectAsset, path);
                AssetDatabase.AddObjectToAsset(layoutAsset, effectAsset);
                AssetDatabase.ImportAsset(path);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                var initial = graphView.Q<EffectMasterNode>(null, EffectGraphController.EffectGraphMasterNodeClassName).EffectPort.connections.Select((x) => x.output.node).ToArray();
                EffectGraphController.SerializeTo(effectGraph, effectAsset, layoutAsset, initial, new NodeReader());
            }
        }

        public void ShowInProject()
        {
            if (instanceId != 0)
            {
                ProjectWindowUtil.ShowCreatedAsset(EditorUtility.InstanceIDToObject(instanceId));
            }
        }
        #endregion
        private void ConnectToMaster(Port port, GraphView graphView)
        {
            graphView.AddElement(port.ConnectTo(graphView.Q<EffectMasterNode>(null, EffectGraphController.EffectGraphMasterNodeClassName).EffectPort));
        }
        private class DoCreateEffect : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            private int layoutInstanceId;
            public static DoCreateEffect CreateInstance(int layoutInstanceId)
            {
                var action = CreateInstance<DoCreateEffect>();
                action.layoutInstanceId = layoutInstanceId;
                return action;
            }
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var effect = EditorUtility.InstanceIDToObject(instanceId);
                var layout = EditorUtility.InstanceIDToObject(layoutInstanceId);
                AssetDatabase.CreateAsset(effect, pathName);
                AssetDatabase.AddObjectToAsset(layout, effect);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(layout));
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
        public class NodeReader : INodeReader
        {
            public Node[] Collect(Node node)
            {
                var arr1 = node.Q<Port>(null, EffectGraphController.OutputPortClassName)?.connections?.Select((x) => x.input.node)?.ToArray() ?? Array.Empty<Node>();
                var arr2 = node.Q<Port>(null, EffectGraphController.InputPortClassName)?.connections?.Select((x) => x.output.node)?.ToArray() ?? Array.Empty<Node>();
                return arr1.Concat(arr2).ToArray();
            }

            public Type GetNodeType(Node node)
            {
                return EffectGraphController.GetNodeType(node.Q<Port>(null, EffectGraphController.InputPortClassName)?.portType);
            }

            public bool IsRoot(Node node)
            {
                var inputPort = node.Q<Port>(null, EffectGraphController.InputPortClassName);
                if (inputPort == null)
                    return false;
                return !inputPort.connected;


            }
        }
    }

}