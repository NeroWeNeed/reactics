using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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
        public ObjectGraphModel Model { get; private set; }

        public ObjectGraphInspector Inspector { get; private set; }

        public Vector2 LastMousePosition { get; private set; }
        private readonly JsonObjectGraphSerializer jsonObjectGraphSerializer = new JsonObjectGraphSerializer();

        public ObjectGraphView(ObjectGraphModel model, Func<Vector2, Vector2> screenToWorldConverter, params IObjectGraphModule[] modules) {
            this.Model = model;
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
            SetupInspector();

            serializeGraphElements = OnSerializeElements;
            unserializeAndPaste = OnDeserializeElements;
            this.RegisterCallback<DragUpdatedEvent>((evt) =>
            {
                var z = DragAndDrop.GetGenericData("DragSelection");
                if (z is List<ISelectable>) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
            });
            this.RegisterCallback<DragPerformEvent>((evt) =>
            {
                if (DragAndDrop.GetGenericData("DragSelection") is List<ISelectable> selectables) {
                    Vector2 offset = new Vector2(0, 0);
                    foreach (var selectable in selectables) {
                        if (selectable is BlackboardField blackboardField && blackboardField.userData is ObjectGraphVariable variableData) {
                            var variable = new ObjectGraphVariableNode(variableData);
                            variable.tooltip = $"{variable.Data.type.GetRealName()}";
                            Model.CreateVariableEntry(variable.Data, variable.viewDataKey);
                            variable.SetPosition(new Rect(contentViewContainer.WorldToLocal(contentViewContainer.parent.ChangeCoordinatesTo(contentViewContainer.parent, evt.mousePosition)) + offset, default));
                            this.AddElement(variable);
                            offset.y += blackboardField.worldBound.height + 2;
                        }
                    }

                }
            });
            graphViewChanged = OnGraphViewChange;
            //SetupUndo();


        }

        public void RefreshInspector(SerializedObject obj) {
            Inspector?.settingsView?.Bind(obj);
            ValidateVariables();
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
        public GraphViewChange OnGraphViewChange(GraphViewChange change) {
            ValidateVariables();
            Debug.Log("Change Detected");
            if (change.elementsToRemove != null) {
                foreach (var vNode in change.elementsToRemove.OfType<ObjectGraphVariableNode>()) {
                    Model.RemoveVariableEntry(vNode.viewDataKey);
                }
                foreach (var edge in change.elementsToRemove.OfType<Edge>()) {
                    Debug.Log("Removing " + edge);
                }
            }

            return change;
        }
        private void ValidateVariables() {
            foreach (var provider in Model.variables.Select((variable) => variable.provider).Distinct()) {

                provider.OnValidateVariables(this, Model.variables.Where((variable) => variable.provider == provider).ToArray());
            }
            Inspector.variableView.Query<VisualElement>(null, ObjectGraphVariableProvider.OBJECT_VARIABLE_FIELD_CLASS_NAME).ForEach((field) =>
            {
                if (field.userData is ObjectGraphVariable variable) {
                    field.style.display = variable.valid ? DisplayStyle.Flex : DisplayStyle.None;
                }
            });

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
        public void Clean<TNode>() where TNode : ObjectGraphNode {
            DeleteElements(this.Query<TNode>().ToList());
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
        public void SetupInspector() {
            var inspector = new ObjectGraphInspector(this);
            Inspector = inspector;
            this.Add(inspector);
            foreach (var module in modules.OfType<IInspectorConfigurator>()) {
                var element = module.CreateInspectorSection(this);
                if (element != null)
                    Inspector.AddInspector(element);
            }
            Inspector.AddVariables(modules.OfType<IVariableProvider>().SelectMany((provider) => provider.VariableTypes).ToArray(), Model);
            ValidateVariables();
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where((port) =>
            {
                if (startPort.ClassListContains(ObjectGraphValuePort.DirectPortClassName)) {
                    return port.ClassListContains(ObjectGraphValuePort.DirectPortClassName) && port.node == startPort.node && port.viewDataKey == startPort.viewDataKey;
                }

                foreach (var module in modules) {
                    if (module is IObjectGraphNodeProvider provider && provider.GetCompatiblePorts(startPort, nodeAdapter, port)) {
                        return true;
                    }
                }
                return false;
            }).ToList();
        }
        public List<ObjectGraphNode> GetRoots(string className) {
            return nodes.ToList().OfType<ObjectGraphNode>().Where((node) => !node.input.connected && node.IsConnected() && node.ClassListContains(className)).ToList();
        }
        public List<TNode> GetRoots<TNode>() where TNode : ObjectGraphNode {
            return nodes.ToList().OfType<TNode>().Where((node) => !node.input.connected && node.IsConnected()).ToList();
        }

        public bool Validate() {
            var result = true;
            foreach (var module in modules.OfType<IObjectGraphValidator>()) {
                if (!module.ValidateGraph(this)) {
                    result = false;
                    break;
                }
            }
            ValidateVariables();
            this.Query<ObjectGraphVariableNode>(null, ObjectGraphVariableNode.OBJECT_GRAPH_VARIABLE_CLASS_NAME).ForEach((node) =>
            {
                if (!node.Data.valid) {
                    node.output.ErrorNotification("Invalid Variable Node");
                    result = false;
                }
                else {
                    node.output.ClearNotifications();
                }
            });
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

    public interface IObjectGraphViewListener {
        void OnGraphViewChange(ObjectGraphView graphView);
    }
}