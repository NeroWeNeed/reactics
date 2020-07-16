using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ObjectGraphView : GraphView {

        public const string USS_GUID = "256dcec08179d5a41bbf70ec00648654";
        private ObjectGraphSearchWindow searchWindow;
        private readonly Func<Vector2, Vector2> screenToWorldConverter;
        public const string EffectGraphMasterNodeClassName = "master-node";
        public Node MasterNode { get; }
        private readonly IObjectGraphModule[] modules;
        public readonly ReadOnlyCollection<IObjectGraphModule> Modules;

        public ObjectGraphModelEditor ModelEditor { get; set; }

        public ObjectGraphModel Model
        {
            get => ModelEditor?.Model; set
            {
                if (ModelEditor != null)
                    ModelEditor.Model = value;
            }
        }

        public ObjectGraphVariableBlackboard VariableBlackboard { get; private set; }
        public Vector2 LastMousePosition { get; private set; }

        private readonly JsonObjectGraphSerializer jsonObjectGraphSerializer = new JsonObjectGraphSerializer();

        public ObjectGraphView(ObjectGraphModelEditor editor, Func<Vector2, Vector2> screenToWorldConverter, params IObjectGraphModule[] modules) {
            this.ModelEditor = editor;
            this.modules = modules;
            Modules = Array.AsReadOnly(modules);
            MasterNode = CreateMasterNode(this.modules);

            this.screenToWorldConverter = screenToWorldConverter;
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID)));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new RectangleSelector());
            style.backgroundColor = new Color(0.109f, 0.109f, 0.109f);
            this.contentViewContainer.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                if (evt.oldRect.width == 0 && evt.oldRect.height == 0)
                    FrameAll();

            });
            this.RegisterCallback<MouseMoveEvent>((evt) => LastMousePosition = contentViewContainer.WorldToLocal(contentViewContainer.parent.ChangeCoordinatesTo(contentViewContainer.parent, evt.mousePosition)));
            SetupSearchWindow();
            SetupVariableBlackboard();
            serializeGraphElements = OnSerializeElements;
            unserializeAndPaste = OnDeserializeElements;
            //graphViewChanged = OnGraphViewChange;
            //SetupUndo();

        }

        public void RefreshInspector(SerializedObject obj) {
            VariableBlackboard?.contentContainer?.Clear();
            foreach (var module in modules.OfType<IInspectorConfigurator>()) {
                var element = module.CreateInspectorSection(obj, this);
                if (VariableBlackboard == null)
                    SetupVariableBlackboard();
                VariableBlackboard.Add(element);
            }
        }
        public string OnSerializeElements(IEnumerable<GraphElement> elements) {
            if (jsonObjectGraphSerializer.Serialize(new JsonObjectGraphCollection
            {
                nodes = elements.OfType<ObjectGraphNode>().ToArray()
            }, null, this, out JsonObjectGraphCollection result)) {
                return result.json;
            }
            else {
                return null;
            }
        }
        /*         public GraphViewChange OnGraphViewChange(GraphViewChange change) {
                    foreach (var item in change.elementsToRemove.OfType<ObjectGraphNode>()) {
                        ModelEditor.DeleteEntry(item);
                    }
                    return change;
                } */




        public void OnDeserializeElements(string operationName, string data) {
            //Undo.RecordObject(ModelEditor.Model, operationName);
            jsonObjectGraphSerializer.Deserialize(new JsonObjectGraphCollection
            {
                json = data
            }, null, this, out JsonObjectGraphCollection result);

            //Undo.FlushUndoRecordObjects();
        }
        private Node CreateMasterNode(params IObjectGraphModule[] modules) {
            var masterNode = new Node
            {
                title = "Master",
                viewDataKey = Guid.NewGuid().ToString(),
                layer = -1
            };
            foreach (var module in modules) {
                if (module is IMasterNodeConfigurator configurator)
                    configurator.ConfigureMaster(masterNode);
            }
            masterNode.AddToClassList(EffectGraphMasterNodeClassName);
            masterNode.capabilities ^= Capabilities.Deletable;
            masterNode.RefreshPorts();
            this.AddElement(masterNode);
            return masterNode;
        }
        public void Clean() {
            DeleteElements(this.Query<GraphElement>().Where((ele) => ele.parent != null && ele.parent is Layer && ele != MasterNode).ToList());
        }
        public void SetupSearchWindow() {
            searchWindow = ScriptableObject.CreateInstance<ObjectGraphSearchWindow>();
            nodeCreationRequest = (context) =>
            {
                searchWindow.graphView = this;
                searchWindow.providers = modules.OfType<IObjectGraphNodeProvider>().ToArray();
                searchWindow.screenToWorldConverter = screenToWorldConverter;
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }
        public void SetupVariableBlackboard() {
            var blackboard = new ObjectGraphVariableBlackboard(this)
            {
                title = "Sample"
            };
            VariableBlackboard = blackboard;
            this.Add(blackboard);

        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where((port) =>
            {
                foreach (var module in modules) {
                    if (module is IObjectGraphNodeProvider provider && provider.GetCompatiblePorts(startPort, nodeAdapter, port)) {
                        return true;
                    }
                }
                return false;
            }).ToList();
        }
        public List<ObjectGraphNode> GetRoots() {
            return nodes.ToList().OfType<ObjectGraphNode>().Where((node) => !node.InputPort.connected && node.IsConnected()).ToList();
        }
        public List<TNode> GetRoots<TNode>() where TNode : ObjectGraphNode {
            return nodes.ToList().OfType<TNode>().Where((node) => !node.InputPort.connected && node.IsConnected()).ToList();
        }
        public bool Validate() {
            var result = true;
            foreach (var module in modules.OfType<IObjectGraphValidator>()) {
                if (!module.ValidateGraph(this)) {
                    result = false;
                    break;
                }
            }
            using (ObjectGraphValidateEvent evt = ObjectGraphValidateEvent.GetPooled(result)) {
                evt.target = this;
                this.SendEvent(evt);
                return result;
            }

            /*             this.nodes.ForEach((node) =>
                        {
                            if (node is ObjectGraphNode objectGraphNode) {
                                foreach (var module in modules) {
                                    module.ValidateNode(objectGraphNode, this);
                                }
                            }
                        }); */
        }

    }
}