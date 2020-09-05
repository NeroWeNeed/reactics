using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ObjectGraphNode : Node {

        public const string InputPortClassName = "input-port";
        public const string OutputPortClassName = "output-port";
        public const string ConfigurableFieldClassName = "config-field";
        public const string ObjectGraphNodeClassName = "object-graph-node";
        public const string NotificationClassName = "notification";
        public const string USS_GUID = "097688b9fab8c3e44abbc0e613cb3444";
        private static readonly MethodInfo RegisterCallback = Array.Find(typeof(CallbackEventHandler).GetMethods(), (method) => method.Name == "RegisterCallback" && method.GetGenericArguments().Length == 1);
        private static void UpdateField<T>(ChangeEvent<T> evt) {
            if (evt.target is VisualElement visualElement) {
                using (ChangeFieldEvent fieldEvt = ChangeFieldEvent.GetPooled(typeof(T), visualElement.viewDataKey, evt.newValue)) {
                    fieldEvt.target = evt.target;
                    visualElement.SendEvent(fieldEvt);
                }
            }
        }

        public Port input;
        public Port output;
        private Type type;
        public Type Type
        {
            get => type;
            set
            {
                if (type != value) {
                    input.portType = value;
                    input.portColor = TypeCommons.GetColor(value);
                    output.portType = value;
                    output.portColor = TypeCommons.GetColor(value);
                    type = value;
                }
            }
        }

        public bool IsRoot { get => !input.connected; }
        public string Id { get => this.viewDataKey; set => viewDataKey = value; }

        private string targetInputPortClassName;

        public string TargetInputPortClassName
        {
            get => targetInputPortClassName;
            set
            {
                if (targetInputPortClassName != value) {
                    input.RemoveFromClassList(targetInputPortClassName);
                    input.AddToClassList(targetInputPortClassName);
                    targetInputPortClassName = value;
                }
            }
        }
        private ObjectGraphView graphView;
        public ObjectGraphView GraphView
        {
            get => graphView; set
            {
                if (graphView == null) {
                    graphView = value;
                }
            }
        }
        public ObjectGraphModel Model => graphView?.Model;
        public ObjectGraphNode() {
            Init();
        }
        private void Init() {
            input = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, null);
            input.portName = "In";
            input.AddToClassList(InputPortClassName);
            input.MakeDependent();
            this.inputContainer.Add(input);

            output = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null);
            output.portName = "Out";
            output.MakeObservable();
            output.RegisterCallback<PortChangedEvent>((evt) =>
            {
                graphView.Model?.SetEntryNext(Id, evt.edges.FirstOrDefault()?.input?.node?.viewDataKey);
                this.GetFirstAncestorOfType<ObjectGraphView>()?.Validate();
            });
            output.AddToClassList(OutputPortClassName);
            this.outputContainer.Add(output);
            RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                Debug.Log(Id);
                if (evt.target is VisualElement element) {
                    graphView = element.GetFirstAncestorOfType<ObjectGraphView>();
                    Refresh();
                }

            });
            RegisterCallback<DetachFromPanelEvent>((evt) =>
            {
                input.ClearEdges();
                output.ClearEdges();
            });
            RegisterCallback<ChangeFieldEvent>((evt) =>
            {
                Model.SetValue(Id, evt.name, evt.value);
                graphView?.Validate();
            });

        }
        public void Refresh() {
            var entry = Model.GetEntry(Id);
            var values = Model?.GetEntryValues(Id);
            foreach (var item in this.Query<VisualElement>(null, ConfigurableFieldClassName).ToList()) {
                item.RemoveFromHierarchy();
            }
            title = Model?.GetEntry(Id).type?.Name;

            foreach (var kv in values) {
                var attrs = entry.type.GetField(kv.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetCustomAttributes()?.ToArray();
                if (attrs == null)
                    attrs = Array.Empty<Attribute>();
                VisualElementDrawer element = VisualElementDrawers.Create(kv.Value.type, kv.Key, kv.Value.value, attrs);
                element.name = kv.Key;
                element.viewDataKey = kv.Key;
                element.AddToClassList(ConfigurableFieldClassName);
                var changeEvent = typeof(ChangeEvent<>).MakeGenericType(kv.Value.type);
                var changeEventCallbackMethod = typeof(ObjectGraphNode).GetMethod("UpdateField", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(kv.Value.type);
                var changeEventCallback = Delegate.CreateDelegate(typeof(EventCallback<>).MakeGenericType(changeEvent), changeEventCallbackMethod);
                RegisterCallback.MakeGenericMethod(changeEvent).Invoke(element, new object[] { changeEventCallback, TrickleDown.NoTrickleDown });
                var layoutAttr = element.GetType().GetCustomAttributes()?.OfType<VisualElementLayout>().FirstOrDefault();
                if (layoutAttr == null)
                    extensionContainer.Add(element);
                else
                    layoutAttr.Layout(element, this);
            }
            if (string.IsNullOrEmpty(entry.next)) {
                output.DisconnectAll();
            }
            else {

                var targetPort = graphView.GetNodeByGuid(entry.next)?.Q<Port>(null, targetInputPortClassName);
                if (((output.connected && output.connections.All((edge) => edge.input != targetPort)) || !output.connected) && targetPort != null) {
                    graphView.AddElement(output.ConnectTo(targetPort));
                }


            }
        }

        public void ErrorNotification(string message) {
            ClearNotifications();
            var badge = IconBadge.CreateError(message);
            badge.AddToClassList(NotificationClassName);
            this.Add(badge);
            badge.AttachTo(this.titleContainer, SpriteAlignment.RightCenter);

        }
        public void Notification(string message) {
            ClearNotifications();
            var badge = IconBadge.CreateComment(message);
            badge.AddToClassList(NotificationClassName);
            this.Add(badge);
            badge.AttachTo(this.titleContainer, SpriteAlignment.RightCenter);
        }
        public void ClearNotifications() {
            this.Query<VisualElement>(null, NotificationClassName).ForEach((element) => element.RemoveFromHierarchy());
        }
        public bool IsConnected() {
            if (outputContainer == null || outputContainer.childCount == 0)
                return false;
            var result = false;
            this.outputContainer.Query<Port>().ForEach((port) => result = result || port.connected);
            return result;
        }
    }
}