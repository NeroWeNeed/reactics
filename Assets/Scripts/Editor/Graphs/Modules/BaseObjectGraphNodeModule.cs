using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {


    public abstract class BaseObjectGraphNodeModule<TNode> : IObjectGraphNodeProvider<TNode>, IMasterNodeConfigurator, IObjectGraphValidator where TNode : ObjectGraphNode, new() {

        public string SearchTreeHeaderName;
        public string PortClassName { get; }
        public string PortName { get; }
        public Color PortColor { get; }
        public Type PortType { get; }
        protected readonly Type[] types;
        protected readonly Type[] superTypes;
        public readonly ReadOnlyCollection<Type> Types;
        public readonly ReadOnlyCollection<Type> SuperTypes;
        public ObjectGraphSerializer<SerializedObject> Serializer { get; }
        private readonly List<TNode> roots = new List<TNode>();
        protected BaseObjectGraphNodeModule(Settings settings) {
            this.PortClassName = settings.portClassName;
            this.PortName = settings.portName;
            this.PortColor = settings.portColor;
            this.PortType = settings.portType;
            this.superTypes = settings.superTypes;
            this.SuperTypes = Array.AsReadOnly(superTypes);
            this.Serializer = settings.serializer;
            this.SearchTreeHeaderName = string.IsNullOrEmpty(settings.searchTreeHeaderName) ? settings.portName : settings.searchTreeHeaderName;
            this.types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((assembly) => assembly.GetTypes().Where((t) => !t.IsGenericType && superTypes.Any((s) => s.IsAssignableFrom(t)) && t?.IsUnmanaged() == true && IsValidType(t))).ToArray();
            this.Types = Array.AsReadOnly(types);
        }
        public ObjectGraphNode[] CollectNodes(ObjectGraphView graphView) => ObjectGraphUtility.CollectNodes<TNode>(graphView.MasterNode.Q<Port>(null, PortClassName));
        public void ConfigureMaster(Node master) {
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, PortType);
            port.portName = PortName;
            port.portColor = PortColor;
            port.AddToClassList(PortClassName);
            master.inputContainer.Add(port);
        }

        public ObjectGraphNode Create(string id, Type type, Rect layout) => IObjectGraphNodeProviderExtensions.Create(this, id, type, layout);
        public ObjectGraphNode Create(ObjectGraphNodeJsonSet.Entry entry) => IObjectGraphNodeProviderExtensions.Create(this, entry);
        public virtual bool IsValidType(Type type) => true;
        public List<SearchTreeEntry> CreateSearchTreeEntries(SearchWindowContext context, int depth) => ObjectGraphModuleUtility.CreateSearchEntries(SearchTreeHeaderName, depth, this, SuperTypes, Types);
        public bool GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter, Port targetPort) {
            return !targetPort.direction.Equals(startPort.direction) && (targetPort.node is TNode || startPort.node is TNode) && ((targetPort.portType == PortType && SuperTypes.Contains(startPort.portType)) || EqualityComparer<Type>.Default.Equals(targetPort.portType, startPort.portType));
        }

        public bool ValidateGraph(ObjectGraphView view) {
            foreach (var root in roots) {
                root.ClearNotifications();
            }
            roots.Clear();
            var masterNodePort = view.MasterNode.Q<Port>(null, PortClassName);
            masterNodePort.ClearNotifications();
            roots.AddRange(view.GetRoots<TNode>());
            if (!Serializer.CanSerialize(this, view, out string message)) {
                foreach (var root in roots) {
                    root.ErrorNotification(message);
                }
                return false;
            }

            if (roots.Count == 0) {
                masterNodePort.ErrorNotification("No Roots found.");
                return false;
            }
            var rootTypes = roots.Select((root) => root.SuperTargetType).Distinct().Count();
            if (rootTypes > 1) {
                foreach (var root in roots) {
                    root.ErrorNotification("Multiple SuperTypes found for root nodes.");
                }
                return false;
            }
            else if (rootTypes == 1) {
                return true;
            }
            else {
                foreach (var root in roots) {
                    root.ErrorNotification("Multiple SuperTypes found for root nodes.");
                }
                return false;
            }

        }

        public struct Settings {
            public ObjectGraphSerializer<SerializedObject> serializer;
            public string portClassName;
            public string portName;
            public Color portColor;
            public Type portType;
            public Type[] superTypes;
            public string searchTreeHeaderName;
        }
    }
}