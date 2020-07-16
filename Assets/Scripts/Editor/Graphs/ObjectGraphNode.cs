using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Reactics.Commons;
using Reactics.Commons.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {


    public abstract class ObjectGraphNode : Node {
        public const string InputPortClassName = "input-port";
        public const string OutputPortClassName = "output-port";
        public const string ConfigurableFieldClassName = "config-field";
        public const string ObjectGraphNodeClassName = "object-graph-node";
        public const string NotificationClassName = "notification";
        public const string USS_GUID = "097688b9fab8c3e44abbc0e613cb3444";
        private static MethodInfo registerCallback = null;
        private static MethodInfo RegisterCallback
        {
            get
            {
                return registerCallback = Array.Find(typeof(CallbackEventHandler).GetMethods(), (method) => method.Name == "RegisterCallback" && method.GetGenericArguments().Length == 1);
            }
        }
        private static void UpdateField<T>(ChangeEvent<T> evt) {
            if (evt.target is VisualElement visualElement) {
                using (ChangeFieldEvent fieldEvt = ChangeFieldEvent.GetPooled(typeof(T), visualElement.viewDataKey, evt.newValue)) {
                    fieldEvt.target = evt.target;
                    visualElement.SendEvent(fieldEvt);
                }
            }
        }
        private Type targetType;
        public Type TargetType
        {
            get => targetType;
            set
            {
                if (value != targetType && value.IsUnmanaged()) {
                    targetType = value;
                    superTargetType = GetSuperTargetType(targetType);
                    ConfigureHeader(value);

                    if (ModelEditor != null) {
                        if (!ModelEditor.TryGetEntry(this, out ObjectGraphModel.Entry entry) || entry.type != value) {
                            ModelEditor.InitEntry(this);
                        }
                        ConstructNodeContent(ModelEditor.GetEntry(this));
                    }
                    else {
                        ConstructNodeContent(value);

                    }
                }

            }
        }
        private Port inputPort;
        private Port outputPort;
        public Port InputPort
        {
            get
            {
                if (inputPort == null)
                    ConfigureInputPort(TargetType);
                return inputPort;
            }
        }
        public Port OutputPort
        {
            get
            {
                if (outputPort == null)
                    ConfigureOutputPort(TargetType);
                return outputPort;
            }
        }
        public ObjectGraphModel.Entry Entry
        {
            get
            {
                var me = ModelEditor;
                if (me == null)
                    return default;
                else
                    return me.GetEntry(this);
            }
            set
            {
                var old = ModelEditor?.GetEntry(this);
                if (!old.Equals(value)) {
                    if (TargetType != value.type) {
                        ConfigureHeader(value.type, value.next);
                        ConstructNodeContent(value);
                    }
                    else {
                        this.ConnectById(OutputPort, value.next);
                        UpdateNodeContent(value);
                    }
                    Debug.Log(value);

                    ModelEditor?.SetEntry(this, value);
                }

            }


        }
        public bool IsRoot
        {
            get => !InputPort.connected;
        }
        public string Id
        {
            get => viewDataKey;
            set => viewDataKey = value;
        }
        private Type superTargetType;
        public Type SuperTargetType
        {
            get => superTargetType;
            set
            {
                if (value == null && TargetType == null) {
                    superTargetType = null;
                }
                else if (value.IsAssignableFrom(TargetType)) {
                    superTargetType = value;
                }

            }
        }


        public ObjectGraphModelEditor ModelEditor { get; internal set; }

        private ObjectGraphView graphView;
        protected ObjectGraphNode(object source, string guid) : this(guid) {
            ConfigureHeader(source.GetType(), null);

        }

        protected ObjectGraphNode(Type type, string guid) : this(guid) {
            TargetType = type;
        }
        protected ObjectGraphNode(ObjectGraphModel.Entry entry, string guid) : this(guid) {
            Entry = entry;
        }
        protected ObjectGraphNode(string guid) {
            Init(guid);
            ConfigureHeader(null, null);
        }
        protected ObjectGraphNode() : this(Guid.NewGuid().ToString()) { }
        private void Init(string guid = null) {
            AddToClassList(ObjectGraphNodeClassName);


            capabilities = Capabilities.Ascendable | Capabilities.Copiable | Capabilities.Deletable | Capabilities.Selectable | Capabilities.Movable | Capabilities.Collapsible;
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID)));
            viewDataKey = string.IsNullOrEmpty(guid) ? Guid.NewGuid().ToString() : guid;
            RegisterCallback<DetachFromPanelEvent>((_) =>
            {
                //Debug.Log($"detach {viewDataKey}");
                var entry = ModelEditor?.GetEntry(this);
                ModelEditor?.DeleteEntry(this);
                graphView?.Validate();
                ModelEditor = null;
                graphView = null;
            });
            RegisterCallback<ChangeFieldEvent>((evt) =>
            {
                ModelEditor.WriteEntryValue(this, evt.name, evt.value);
                graphView?.Validate();
            });

            RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                //Debug.Log($"attach {viewDataKey}");
                var graphView = this.GetFirstAncestorOfType<ObjectGraphView>();
                if (ModelEditor == null)
                    ModelEditor = graphView?.ModelEditor;


                ModelEditor?.InitIfMissing(this);
                SyncWithEntry();
                graphView?.Validate();
                this.graphView = graphView;




            });
            ConfigureHeader();
        }




        public void SyncWithEntry() {
            if (ModelEditor != null && ModelEditor.TryGetEntry(this, out ObjectGraphModel.Entry entry)) {
                if (TargetType != entry.type) {
                    ConfigureHeader(entry.type, entry.next);
                    ConstructNodeContent(entry);
                }
                else {
                    Debug.Log("CONNECT");
                    this.ConnectById(OutputPort, entry.next);

                    UpdateNodeContent(entry);
                }
                //ConstructNodeContent(entry);
            }
        }
        private void UpdateNodeContent(ObjectGraphModel.Entry entry) {
            Debug.Log($"Updating with {entry}");
            this.Query<VisualElementDrawer>(null, ConfigurableFieldClassName).ForEach((field) =>
            {
                if (entry.values.TryGetValue(field.viewDataKey, out object value)) {
                    field.TrySetValue(value);
                }
            });
            this.RefreshExpandedState();
            this.RefreshPorts();
        }

        private void ConstructNodeContent(ObjectGraphModel.Entry entry) {

            this.Query<VisualElement>(null, ConfigurableFieldClassName).ForEach((element) => element.RemoveFromHierarchy());
            foreach (var key in entry.values.Keys.ToArray()) {
                ConstructNodeContent(key, entry.values[key].GetType(), entry.values[key], TargetType.GetField(key).GetCustomAttributes().ToArray());
            }
            this.RefreshExpandedState();
            this.RefreshPorts();





        }
        private void ConstructNodeContent(Type type) {
            this.Query<VisualElement>(null, ConfigurableFieldClassName).ForEach((element) => element.RemoveFromHierarchy());
            foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (!fieldInfo.IsSerializableField())
                    continue;
                var fieldValue = fieldInfo.FieldType.IsValueType ? Activator.CreateInstance(fieldInfo.FieldType) : null;
                ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, this, out fieldValue);
                ConstructNodeContent(fieldInfo.Name, fieldValue.GetType(), fieldValue, fieldInfo.GetCustomAttributes().ToArray());
            }
            this.RefreshExpandedState();
            this.RefreshPorts();
        }
        private void ConstructNodeContent(object source) {
            this.Query<VisualElement>(null, ConfigurableFieldClassName).ForEach((element) => element.RemoveFromHierarchy());
            foreach (var fieldInfo in source.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (!fieldInfo.IsSerializableField())
                    continue;
                var fieldValue = source == null ? Activator.CreateInstance(fieldInfo.FieldType) : fieldInfo.GetValue(source);
                ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, this, out fieldValue);
                ConstructNodeContent(fieldInfo.Name, fieldValue.GetType(), fieldValue, fieldInfo.GetCustomAttributes().ToArray());
            }
            this.RefreshExpandedState();
            this.RefreshPorts();
        }
        private void ConstructNodeContent(string name, Type valueType, object value, Attribute[] attributes) {
            VisualElement element = VisualElementDrawers.Create(valueType, name, value, attributes);
            element.name = name;
            element.viewDataKey = name;
            element.AddToClassList(ConfigurableFieldClassName);
            var changeEvent = typeof(ChangeEvent<>).MakeGenericType(valueType);
            var changeEventCallbackMethod = typeof(ObjectGraphNode).GetMethod("UpdateField", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(valueType);
            var changeEventCallback = Delegate.CreateDelegate(typeof(EventCallback<>).MakeGenericType(changeEvent), changeEventCallbackMethod);
            RegisterCallback.MakeGenericMethod(changeEvent).Invoke(element, new object[] { changeEventCallback, TrickleDown.NoTrickleDown });
            var layoutAttr = element.GetType().GetCustomAttributes()?.OfType<VisualElementLayout>().FirstOrDefault();
            if (layoutAttr == null)
                extensionContainer.Add(element);
            else
                layoutAttr.Layout(element, this);

        }


        private void ConstructNodeContent() => ConstructNodeContent(ModelEditor.GetEntry(this));

        private void CollectConnectedEdges(HashSet<GraphElement> edgeSet) {
            edgeSet.UnionWith((from d in inputContainer.Query<Port>().ToList().SelectMany((Port c) => c.connections)
                               where (d.capabilities & Capabilities.Deletable) != 0
                               select d).Cast<GraphElement>());
            edgeSet.UnionWith((from d in outputContainer.Query<Port>().ToList().SelectMany((Port c) => c.connections)
                               where (d.capabilities & Capabilities.Deletable) != 0
                               select d).Cast<GraphElement>());
        }

        public override void CollectElements(HashSet<GraphElement> collectedElementSet, Func<GraphElement, bool> conditionFunc) => CollectConnectedEdges(collectedElementSet);
        public abstract Edge ConnectToMaster(Port port, Node master);
        private void ConfigureInputPort(Type type) {
            var inputPort = this.Q<Port>(null, InputPortClassName);
            if (inputPort == null) {

                inputPort = CreatePort(type == null ? null : GetPortType(type), "In", Direction.Input, Port.Capacity.Multi);
                inputPort.AddToClassList(InputPortClassName);
                inputContainer.Add(inputPort);
            }
            else {
                inputPort.portType = type == null ? null : GetPortType(type);
                inputPort.portColor = GetPortColor(type);
            }
            this.inputPort = inputPort;
        }
        private void ConfigureOutputPort(Type type) {
            var outputPort = this.Q<Port>(null, OutputPortClassName);
            if (outputPort == null) {
                outputPort = CreatePort(type == null ? null : GetPortType(type), "Out", Direction.Output, Port.Capacity.Single);
                outputPort.MakeObservable();
                outputPort.RegisterCallback<PortChangedEvent>((evt) =>
                {
                    ModelEditor?.WriteNext(this, evt.edges.FirstOrDefault()?.input?.node?.viewDataKey);
                    this.GetFirstAncestorOfType<ObjectGraphView>()?.Validate();
                });
                outputPort.AddToClassList(OutputPortClassName);
                outputContainer.Add(outputPort);
            }
            else {
                outputPort.portType = type == null ? null : GetPortType(type);
                outputPort.portColor = GetPortColor(type);
            }

            this.outputPort = outputPort;
        }

        private void ConfigureHeader(Type type, string next) {
            title = type?.Name;

            ConfigureInputPort(type);
            ConfigureOutputPort(type);
            this.ConnectById(OutputPort, next);
        }
        private void ConfigureHeader(Type type) {
            title = type?.Name;
            ConfigureInputPort(type);
            ConfigureOutputPort(type);
        }
        private void ConfigureHeader() {
            title = "";
            ConfigureInputPort(null);
            ConfigureOutputPort(null);
        }
        protected virtual Type GetPortType(Type type) => GetSuperTargetType(type);
        public abstract Type GetSuperTargetType(Type type);
        protected abstract Color GetPortColor(Type type);
        public Port CreatePort(Type type, string name, Direction direction, Port.Capacity capacity) {
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, capacity, type == null ? null : type);
            port.portColor = GetPortColor(type);
            port.portName = name;
            return port;
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