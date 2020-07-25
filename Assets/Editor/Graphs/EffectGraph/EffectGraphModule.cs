using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reactics.Battle;
using Reactics.Battle.Map;
using Reactics.Commons;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class EffectGraphModule : BaseObjectGraphNodeModule<EffectGraphNode>, IObjectGraphPostDeserializerCallback, IObjectGraphPreSerializerCallback, IObjectGraphPostSerializerCallback, IObjectGraphValidator {
        public static readonly Type[] SuperTypes = { typeof(IEffect<Point>), typeof(IEffect<MapBodyDirection>), typeof(IEffect<MapBodyTarget>) };
        public const string PortClassName = "effect-graph-node-port";
        static EffectGraphModule() {
            ObjectGraphNodePort.RegisterColor(typeof(IEffect<Point>), Color.cyan);
            ObjectGraphNodePort.RegisterColor(typeof(IEffect<MapBodyDirection>), Color.yellow);
            ObjectGraphNodePort.RegisterColor(typeof(IEffect<MapBodyTarget>), Color.magenta);
        }

        public EffectGraphModule() : this(new Settings
        {
            portName = "Effects",
            portType = typeof(IEffect),
            portClassName = PortClassName,
            portColor = new Color(1f, 0.498f, 0f),
            superTypes = SuperTypes,
            serializer = new UnityObjectGraphSerializer
            {
                dataPropertyPath = "effect",
                rootPropertyPath = "roots",
                variablePropertyPath = "variables"

            }
        }) {

        }
        public EffectGraphModule(Settings settings) : base(settings) { }
        /*         public bool ValidateGraph(ObjectGraphView view) {
                    foreach (var root in roots) {
                        root.ClearNotifications();
                    }
                    roots.Clear();
                    var masterNodePort = view.MasterNode.Q<Port>(null, PortClassName);
                    masterNodePort.ClearNotifications();
                    roots.AddRange(view.GetRoots<EffectGraphNode>());
                    if (roots.Count == 0) {
                        masterNodePort.ErrorNotification("No Roots found.");
                        return false;
                    }
                    var rootTypes = roots.Select((root) => root.SuperTargetType).Distinct().Count();
                    if (rootTypes > 1) {
                        foreach (var root in roots) {
                            root.ErrorNotification("Multiple Matching Types found for root nodes");
                        }
                        return false;
                    }
                    else if (rootTypes == 1) {
                        return true;
                    }
                    else {
                        foreach (var root in roots) {
                            root.ErrorNotification("All root nodes must be of the same type.");
                        }
                        return false;
                    }
                } */
        public void OnPostDeserialize(SerializedObject obj, ObjectGraphView graphView, ref SortedDictionary<string, ObjectGraphModel.Entry> entries) {
            var processed = new HashSet<string>();
            var ids = entries.Keys.ToArray();
            var oldEntries = entries.Values.ToArray();

            entries.Clear();
            for (int i = 0; i < ids.Length; i++) {
                if (processed.Contains(ids[i]))
                    continue;
                if (oldEntries[i].type.Equals(typeof(LinearEffect))) {
                    int next = (int)oldEntries[i].values["next"];
                    int effect = (int)oldEntries[i].values["effect"];
                    var target = oldEntries[effect];
                    if (next == -2) {
                        target.next = graphView.MasterNode.viewDataKey;
                    }
                    else if (next >= 0) {
                        target.next = ids[next];
                    }
                    else {
                        target.next = null;
                    }
                    entries[ids[i]] = target;

                    processed.Add(ids[effect]);

                }
                else {
                    entries[ids[i]] = oldEntries[i];
                }


            }
        }

        public void OnPreSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload) {
            var data = payload.entries.Select((e) => e.data).ToList();
            for (int i = 0; i < payload.entries.Count; i++) {
                var payloadEntry = payload.entries[i].entry;
                var payloadData = payload.entries[i].data;
                if (string.IsNullOrEmpty(payloadEntry.next))
                    continue;
                if (payloadEntry.next == payload.graphView.MasterNode.viewDataKey) {
                    data.Add(payloadData);
                    data[i] = new LinearEffect
                    {
                        effect = data.Count - 1,
                        next = -2
                    };
                }
                else {
                    var index = payload.entries.FindIndex((e) => e.key == payloadEntry.next);
                    if (index >= 0) {
                        data.Add(payloadData);
                        data[i] = new LinearEffect
                        {
                            effect = data.Count - 1,
                            next = index
                        };
                    }
                }
            }

            for (int i = 0; i < data.Count; i++) {
                if (i < payload.entries.Count) {
                    payload.entries[i] = new ObjectGraphSerializerPayload.Entry
                    {
                        key = payload.entries[i].key,
                        data = data[i],
                        entry = payload.entries[i].entry,
                        node = payload.entries[i].node
                    };
                }
                else {
                    payload.entries.Add(new ObjectGraphSerializerPayload.Entry
                    {
                        key = null,
                        data = data[i],
                        entry = default,
                        node = null
                    });
                }
            }
        }

        public void OnPostSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload) {
            var property = obj.FindProperty("type");
            var targetType = payload.graphView.GetRoots<EffectGraphNode>()?.FirstOrDefault()?.SuperTargetType;
            if (typeof(IEffect<>).IsAssignableFrom(targetType)) {
                property.enumValueIndex = (int)TargetTypeUtility.GetType(targetType.GenericTypeArguments[0]);
            }
            else {
                property.enumValueIndex = 0;
            }
        }



    }
    /*     public class EffectGraphModule : IObjectGraphModule, IObjectGraphNodeProvider<EffectGraphNode>, IObjectGraphPostDeserializerCallback, IObjectGraphPreSerializerCallback, IMasterNodeConfigurator {

            public static readonly Type[] Types = { typeof(IEffect<Point>), typeof(IEffect<MapBodyDirection>), typeof(IEffect<MapBodyTarget>) };
            static EffectGraphModule() {
                ObjectGraphNodePort.RegisterColor(typeof(IEffect<Point>), Color.cyan);
                ObjectGraphNodePort.RegisterColor(typeof(IEffect<MapBodyDirection>), Color.yellow);
                ObjectGraphNodePort.RegisterColor(typeof(IEffect<MapBodyTarget>), Color.magenta);
            }

            public static Color GetPortColor(Type type) {
                return ObjectGraphNodePort.GetColor(ObjectGraphModuleUtility.GetTargetType(typeof(IEffect<>), type, Types));
            }

            public static bool TryGetPortColor(Type type, out Color color) {
                return ObjectGraphNodePort.TryGetColor(ObjectGraphModuleUtility.GetTargetType(typeof(IEffect<>), type, Types), out color);
            }
            public static Type GetPortType(Type type) {
                var r = ObjectGraphModuleUtility.GetTargetType(typeof(IEffect<>), type, Types);
                if (r != null) {
                    return r;
                }
                else {
                    throw new ArgumentException($"Unsupported Type: {r}", "type");
                }
            }
            public const string PortClassName = "effect-graph-node-port";
            private readonly List<Type> validTypes;
            public ReadOnlyCollection<Type> ValidTypes { get; }
            public ObjectGraphSerializer<SerializedObject> Serializer { get; }

            private readonly List<EffectGraphNode> roots = new List<EffectGraphNode>();

            public EffectGraphModule() {
                Serializer = new UnityObjectGraphMultiRootSerializer
                {
                    dataPropertyPath = "effect",
                    rootPropertyPath = "roots",
                    variablePropertyPath = "variables"

                };
                validTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany((assembly) => assembly.GetTypes().Where(IsValidType)).ToList();
                ValidTypes = validTypes.AsReadOnly();
            }
            public static bool IsValidType(Type type) {
                return type?.IsUnmanaged() == true && typeof(IEffect).IsAssignableFrom(type) && !typeof(IUtilityEffect).IsAssignableFrom(type);
            }

            public void ConfigureMaster(Node master) {
                var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(IEffect));
                port.portName = "Effect";
                port.portColor = new Color(1f, 0.498f, 0f);
                port.AddToClassList(PortClassName);
                master.inputContainer.Add(port);
            }
            public bool GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter, Port targetPort) {
                return !targetPort.direction.Equals(startPort.direction) && (targetPort.node is EffectGraphNode || startPort.node is EffectGraphNode) && ((targetPort.portType == typeof(IEffect) && Types.Contains(startPort.portType)) || EqualityComparer<Type>.Default.Equals(targetPort.portType, startPort.portType));
            }

            public ObjectGraphNode Create(string id, Type type, Rect layout) => IObjectGraphNodeProviderExtensions.Create(this, id, type, layout);
            public List<SearchTreeEntry> CreateSearchTreeEntries(SearchWindowContext context, int depth) => ObjectGraphModuleUtility.CreateSearchEntries("Effects", depth, this, Types, ValidTypes);
            public ObjectGraphNode[] CollectNodes(ObjectGraphView graphView) => ObjectGraphUtility.CollectNodes<EffectGraphNode>(graphView.MasterNode.Q<Port>(null, PortClassName));
            public ObjectGraphNode Create(ObjectGraphNodeJsonSet.Entry entry) => IObjectGraphNodeProviderExtensions.Create(this, entry);

            public override bool ValidateGraph(ObjectGraphView view) {
                foreach (var root in roots) {
                    root.ClearNotifications();
                }
                roots.Clear();
                var masterNodePort = view.MasterNode.Q<Port>(null, PortClassName);
                masterNodePort.ClearNotifications();
                view.nodes.ForEach((node) =>
                {
                    if (node is EffectGraphNode effectGraphNode && !effectGraphNode.InputPort.connected && effectGraphNode.IsConnected()) {
                        roots.Add(effectGraphNode);
                    }
                });
                if (roots.Count == 0) {
                    masterNodePort.ErrorNotification("No Roots found.");
                    return false;
                }
                var targetTypes = roots.Select((root) => ObjectGraphModuleUtility.GetTargetTypes(typeof(IEffect<>), root.TargetType, Types)).Aggregate((initial, next) => initial.Intersect(next).ToArray()).ToArray();
                if (targetTypes.Length > 1) {
                    foreach (var root in roots) {
                        root.ErrorNotification("Multiple Matching Types found for root nodes");
                    }
                    return false;
                }
                else if (targetTypes.Length == 1) {
                    return true;
                }
                else {
                    foreach (var root in roots) {
                        root.ErrorNotification("All root nodes must be of the same type.");
                    }
                    return false;
                }
            }

            public void OnPostDeserialize(SerializedObject obj, ObjectGraphView graphView, ref SortedDictionary<string, ObjectGraphModel.Entry> entries) {
                var processed = new HashSet<string>();
                var ids = entries.Keys.ToArray();
                var oldEntries = entries.Values.ToArray();

                entries.Clear();
                for (int i = 0; i < ids.Length; i++) {
                    if (processed.Contains(ids[i]))
                        continue;
                    if (oldEntries[i].type.Equals(typeof(LinearEffect))) {
                        int next = (int)oldEntries[i].values["next"];
                        int effect = (int)oldEntries[i].values["effect"];
                        var target = oldEntries[effect];
                        if (next == -2) {
                            target.next = graphView.MasterNode.viewDataKey;
                        }
                        else if (next >= 0) {
                            target.next = ids[next];
                        }
                        else {
                            target.next = null;
                        }
                        entries[ids[i]] = target;

                        processed.Add(ids[effect]);

                    }
                    else {
                        entries[ids[i]] = oldEntries[i];
                    }


                }
            }

            public void OnPreSerialize(SerializedObject obj, ref ObjectGraphSerializerPayload payload) {
                var data = payload.entries.Select((e) => e.data).ToList();
                for (int i = 0; i < payload.entries.Count; i++) {
                    var payloadEntry = payload.entries[i].entry;
                    var payloadData = payload.entries[i].data;
                    if (string.IsNullOrEmpty(payloadEntry.next))
                        continue;
                    if (payloadEntry.next == payload.graphView.MasterNode.viewDataKey) {
                        data.Add(payloadData);
                        data[i] = new LinearEffect
                        {
                            effect = data.Count - 1,
                            next = -2
                        };
                    }
                    else {
                        var index = payload.entries.FindIndex((e) => e.key == payloadEntry.next);
                        if (index >= 0) {
                            data.Add(payloadData);
                            data[i] = new LinearEffect
                            {
                                effect = data.Count - 1,
                                next = index
                            };
                        }
                    }
                }

                for (int i = 0; i < data.Count; i++) {
                    if (i < payload.entries.Count) {
                        payload.entries[i] = new ObjectGraphSerializerPayload.Entry
                        {
                            key = payload.entries[i].key,
                            data = data[i],
                            entry = payload.entries[i].entry,
                            node = payload.entries[i].node
                        };
                    }
                    else {
                        payload.entries.Add(new ObjectGraphSerializerPayload.Entry
                        {
                            key = null,
                            data = data[i],
                            entry = default,
                            node = null
                        });
                    }
                }
            }


        } */
}