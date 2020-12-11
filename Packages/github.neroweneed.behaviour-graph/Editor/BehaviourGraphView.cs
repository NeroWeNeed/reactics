using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons.Editor;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.BehaviourGraph.Editor {
    public class BehaviourGraphView : GraphView, IDisposable {
        public const string BEHAVIOUR_NODE_CLASS = "behaviour-node";
        public const string USS = "Packages/github.neroweneed.behaviour-graph/Editor/Resources/BehaviourGraph.uss";
        public const string BEHAVIOUR_VARIABLE_CLASS = "behaviour-variable";
        public const string VARIABLE_NODE_ICON_PATH = "Packages/github.neroweneed.behaviour-graph/Editor/Resources/VariableNodeIcon.png";
        public static readonly Texture ICON = AssetDatabase.LoadAssetAtPath<Texture>(VARIABLE_NODE_ICON_PATH);
        private List<BehaviourGraphModel> models;
        private BehaviourGraphInspector inspector;
        private BehaviourGraphSearchWindow searchWindow;
        public BehaviourGraphView(BaseBehaviourGraphEditor editor) {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new RectangleSelector());
            this.contentViewContainer.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                if (evt.oldRect.width == 0 && evt.oldRect.height == 0)
                    FrameAll();
            });
            SetupSearchWindow(editor);
            SetupInspector();
            this.graphViewChanged += SendPortChangeEvents;
            this.graphViewChanged += DisposeDeletedElements;
            this.graphViewChanged += UpdateModel;

            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(USS));
            this.RegisterCallback<DragUpdatedEvent>((evt) =>
            {
                var z = DragAndDrop.GetGenericData("DragSelection");
                if (z is List<ISelectable>) {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
            });
            this.RegisterCallback<DragPerformEvent>(evt =>
            {
                if (DragAndDrop.GetGenericData("DragSelection") is List<ISelectable> selectables) {
                    Vector2 offset = new Vector2(0, 0);
                    var pos = evt.mousePosition;
                    foreach (var selectable in selectables) {
                        if (selectable is BlackboardField blackboardField && blackboardField.userData is VariableInfo variableInfo) {
                            var entry = CreateVariable(variableInfo.fieldOffsetInfo, contentViewContainer.WorldToLocal(contentViewContainer.parent.ChangeCoordinatesTo(contentViewContainer.parent, evt.mousePosition)) + offset);
                            variableInfo.model.Entries.Add(entry);
                            var node = entry.CreateNode(this, variableInfo.model.Settings);
                            if (node is IBehaviourGraphNode behaviourGraphNode) {
                                behaviourGraphNode.Model = variableInfo.model;
                            }
                            this.AddElement(node);
                            offset.y += blackboardField.worldBound.height + 2;
                        }
                    }

                }
            });
        }

        private GraphViewChange SendPortChangeEvents(GraphViewChange change) {
            change.edgesToCreate?.ForEach(edge =>
            {
                if (edge.input?.userData is IPortUpdater inputUpdater) {
                    inputUpdater.OnConnect(edge.input, edge);

                }
                if (edge.output?.userData is IPortUpdater outputUpdater) {
                    outputUpdater.OnConnect(edge.output, edge);
                }
            });
            var edges = change.elementsToRemove?.OfType<Edge>();
            if (edges != null) {
                foreach (var edge in edges) {
                    if (edge.input?.userData is IPortUpdater inputUpdater) {
                        inputUpdater.OnDisconnect(edge.input, edge);
                    }
                    if (edge.output?.userData is IPortUpdater outputUpdater) {
                        outputUpdater.OnDisconnect(edge.output, edge);
                    }
                }
            }
            return change;
        }
        private GraphViewChange DisposeDeletedElements(GraphViewChange change) {
            DisposeDeletedElements(change.elementsToRemove?.SelectMany(element => element.Query<ValueTypeElement>(null).ToList()));
            return change;
        }
        private GraphViewChange UpdateModel(GraphViewChange change) {
            var movedNodes = change.movedElements?.OfType<IBehaviourGraphNode>();
            if (movedNodes != null) {
                foreach (var movedElement in movedNodes) {
                    var entry = movedElement.Model.Entries.Find(entry => entry != null && entry?.Id == ((Node)movedElement).viewDataKey);
                    if (entry != null) {
                        entry.Layout = ((Node)movedElement).GetPosition();
                    }
                }
            }
            var removedNodes = change.elementsToRemove?.OfType<IBehaviourGraphNode>();
            if (removedNodes != null) {
                foreach (var removedElement in removedNodes) {
                    var index = removedElement.Model.Entries.FindIndex(entry => entry != null && entry?.Id == ((Node)removedElement).viewDataKey);
                    if (index > 0) {
                        removedElement.Model.Entries[index] = null;
                    }
                }
            }
            return change;
        }
        private void DisposeDeletedElements(IEnumerable<ValueTypeElement> elements) {
            if (elements != null) {
                foreach (var element in elements) {
                    element.Dispose();
                }
            }


        }

        private void SetupSearchWindow(BaseBehaviourGraphEditor editor) {
            searchWindow = ScriptableObject.CreateInstance<BehaviourGraphSearchWindow>();
            if (editor.Models?.Count > 0) {
                searchWindow.models.AddRange(editor.Models);

                nodeCreationRequest = (context) =>
                {
                    searchWindow.graphView = this;
                    searchWindow.editor = editor;
                    SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
                };
            }
        }

        private void SetupInspector() {
            inspector = new BehaviourGraphInspector(this);
            this.Add(inspector);

        }

        public void Init(List<BehaviourGraphModel> models, BaseBehaviourGraphEditor editor) {
            foreach (var model in models) {
                this.DeleteElements(this.graphElements.ToList().Where(element => !(element is BehaviourGraphInspector)));
                if (model == null)
                    continue;
                model.Entries.Sort();
                for (int index = 0; index < model.Entries.Count; index++) {
                    var node = model.Entries[index].CreateNode(this, model.Settings);
                    if (node is IBehaviourGraphNode behaviourGraphNode) {
                        behaviourGraphNode.Model = model;
                        node.RefreshExpandedState();
                        node.RefreshPorts();
                        AddElement(node);
                    }

                }
                InitVariables(model);
                FrameAll();
            }

        }
        private void InitVariables(BehaviourGraphModel model) {
            inspector.VariableContainer.Clear();
            if (model.Settings.variableDefinition.Value != null) {
                foreach (var variable in model.Settings.variableDefinition.Value.Decompose()) {
                    var blackboardField = new BlackboardField(ICON, $"{model.Settings.variableDefinition.Value.Name}.{variable.fullName}", $"{variable.type.Value.FullName}")
                    {
                        capabilities = Capabilities.Selectable | Capabilities.Deletable | Capabilities.Droppable
                    };
                    blackboardField.userData = new VariableInfo { fieldOffsetInfo = variable, model = model };
                    blackboardField.AddToClassList(BEHAVIOUR_VARIABLE_CLASS);
                    blackboardField.Q<Image>("icon").tintColor = variable.type.Value.GetColor();
                    inspector.VariableContainer.Add(blackboardField);

                }
            }
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            var genericTypeDefinition = startPort.portType.GetGenericTypeDefinition();
            var path = startPort.GetPath();
            if (genericTypeDefinition == typeof(BehaviourPort<>)) {
                return ports.ToList().Where(target => target.direction != startPort.direction && target.portType == startPort.portType && !path.Contains(target.node)).ToList();
            }
            else if (genericTypeDefinition == typeof(VariableFieldPort<,>)) {
                return ports.ToList().Where(target => target.direction != startPort.direction && target.portType == startPort.portType && !path.Contains(target.node)).ToList();
            }
            return new List<Port>();
        }

        public void Dispose() {
            this.DisposeDeletedElements(this.graphElements.ToList().SelectMany(element => element.Query<ValueTypeElement>(null).ToList()));
        }

        public virtual void InitializeModel(BehaviourGraphModel model) {
            model.Entries.Add(new MasterEntry());
        }
        public virtual IEntry CreateVariable(FieldOffsetInfo info, Vector2 coordinates) {
            return new VariableEntry(info, new Rect(coordinates, default));
        }
        public virtual IEntry CreateBehaviour(string identifier, BehaviourGraphSettings settings, Vector2 coordinates) {
            return new BehaviourEntry(layout: new Rect(coordinates, new Vector2(100, 100)), settings, behaviourIdentifier: identifier);
        }
        public struct VariableInfo {
            public FieldOffsetInfo fieldOffsetInfo;
            public BehaviourGraphModel model;
        }

    }
    public interface IPortUpdater {

        public void OnConnect(Port source, Edge edge);
        public void OnDisconnect(Port source, Edge edge);
    }

}