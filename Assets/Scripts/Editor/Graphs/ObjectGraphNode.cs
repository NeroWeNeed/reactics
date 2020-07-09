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

namespace Reactics.Editor.Graph
{


    public abstract class ObjectGraphNode : Node
    {
        public const string InputPortClassName = "input-port";
        public const string OutputPortClassName = "output-port";
        public const string ConfigurableFieldClassName = "config-field";
        public const string USS_GUID = "097688b9fab8c3e44abbc0e613cb3444";
        protected VisualElement configContainer = new VisualElement
        {
            name = "config"
        };



        //public object Value { get; private set; }
        private Type targetType;
        public Type TargetType
        {
            get => targetType;
            set
            {
                if (value != targetType && value.IsUnmanaged())
                {
                    targetType = value;
                    if (panel != null)
                    {

                        GetModelEditor().InitEntry(this);
                        UpdateContentContainer(value);
                    }
                }
            }
        }
        private BaseObjectGraphModelEditor modelEditor;
        public ObjectGraphNode(object source, Guid guid)
        {
            Init(source.GetType(), guid);
            UpdateContentContainerWithData(source);
        }

        public ObjectGraphNode(Type type, Guid guid)
        {

            Init(type, guid);

        }



        private void Init(Type type, Guid guid)
        {
            this.Q<VisualElement>("node-border").Add(configContainer);
            capabilities = Capabilities.Ascendable | Capabilities.Copiable | Capabilities.Deletable | Capabilities.Selectable | Capabilities.Movable;
            this.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(USS_GUID)));
            viewDataKey = guid.ToString();
            TargetType = type;



            var outputPort = new ObservablePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, GetPortType(TargetType))
            {
                portColor = GetPortColor(TargetType),
                portName = "Out"
            };
            outputPort.RegisterCallback<PortChangedEvent>((evt) =>
            {
                GetModelEditor().WriteNext(this, evt.edges?.FirstOrDefault()?.input?.node?.viewDataKey);
            });
            this.RegisterCallback<DetachFromPanelEvent>((evt) =>
            {

                (evt.target as ObjectGraphNode).GetModelEditor().DeleteEntry(this);
            });
            this.RegisterCallback<AttachToPanelEvent>((evt) =>
            {

                GetModelEditor().InitEntry(this);
                UpdateContentContainer(TargetType);
            });
            outputPort.AddToClassList(OutputPortClassName);
            /*             if (entry.NextId != Guid.Empty)
                        {
                            var port = graphView.GetElementByGuid(entry.NextId.ToString())?.Q<Port>(null, InputPortClassName);
                            if (port != null)
                            {
                                graphView.AddElement(outputPort.ConnectTo(port));
                            }
                        } */

            outputContainer.Add(outputPort);
            RegisterCallback<ChangeFieldEvent>((evt) =>
            {
                if (evt.value is NodeIndex)
                    return;
                var evtNode = evt.currentTarget as ObjectGraphNode;

                if (evtNode.FindAncestorUserData() is BaseObjectGraphModelEditor modelEditor)
                {
                    modelEditor.WriteEntryValue(evtNode, evt.name, evt.value);
                }
                //Undo.RecordObject(target, $"Set Entry({evtId}) Field {evt.name} to {evt.value}");
                //TargetType.GetField(evt.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Value, evt.value);
            });
        }
        private BaseObjectGraphModelEditor GetModelEditor()
        {
            if (modelEditor == null && panel != null)
                modelEditor = FindAncestorUserData() as BaseObjectGraphModelEditor;
            return modelEditor;
        }
        private void ConstructNodeContent(object source = null)
        {

            this.Query<VisualElement>(null, ConfigurableFieldClassName).ToList().ForEach((element) => element.RemoveFromHierarchy());
            var callback = GetRegisterCallback();
            var entry = GetModelEditor().GetEntry(this);
            foreach (var kv in entry.values)
            {

                VisualElement element;
                element = VisualElementProviders.Create(kv.Value.GetType(), kv.Key, kv.Value, TargetType.GetField(kv.Key).GetCustomAttributes().ToArray());

                element.viewDataKey = kv.Key;
                element.AddToClassList(ConfigurableFieldClassName);
                var changeEvent = typeof(ChangeEvent<>).MakeGenericType(kv.Value.GetType());
                var changeEventCallbackMethod = typeof(ObjectGraphNode).GetMethod("UpdateField", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(kv.Value.GetType());
                var changeEventCallback = Delegate.CreateDelegate(typeof(EventCallback<>).MakeGenericType(changeEvent), changeEventCallbackMethod);
                callback.MakeGenericMethod(changeEvent).Invoke(element, new object[] { changeEventCallback, TrickleDown.NoTrickleDown });
                if (element.GetType().GetCustomAttribute<OutputContainerElement>() != null)
                    outputContainer.Add(element);
                else
                    configContainer.Add(element);

            }

        }




        private void UpdateContentContainer(Type type, object source = null)
        {
            if (!type.IsUnmanaged())
                throw new ArgumentException("Type must be unmanaged", "type");
            if (source != null && !type.IsAssignableFrom(source.GetType()))
                throw new ArgumentException("Type and source mismatch");
            title = type.Name;
            var inputPort = this.Q<Port>(null, InputPortClassName);
            if (inputPort == null)
            {
                inputPort = CreatePort(GetPortType(type), "In", Direction.Input, Port.Capacity.Multi);
                inputPort.AddToClassList(InputPortClassName);
                inputContainer.Add(inputPort);
            }
            else
            {
                inputPort.portType = GetPortType(type);
            }
            ConstructNodeContent(source);
        }

        private void UpdateContentContainerWithData(object data)
        {
            UpdateContentContainer(data.GetType(), data);
        }
        protected abstract Type GetPortType(Type type);

        protected abstract Color GetPortColor(Type type);
        private Port CreatePort(Type type, string name, Direction direction, Port.Capacity capacity)
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, capacity, type);
            port.portColor = GetPortColor(type);
            port.portName = name;
            return port;
        }
        private static MethodInfo GetRegisterCallback() => typeof(CallbackEventHandler).GetMethods().Where((method) => method.Name == "RegisterCallback" && method.GetGenericArguments().Length == 1).FirstOrDefault();
        private static void UpdateField<T>(ChangeEvent<T> evt)
        {
            if (evt.target is VisualElement visualElement)
            {
                using (ChangeFieldEvent fieldEvt = ChangeFieldEvent.GetPooled(typeof(T), visualElement.viewDataKey, evt.newValue))
                {
                    fieldEvt.target = evt.target;
                    visualElement.SendEvent(fieldEvt);
                }
            }
        }


    }


}