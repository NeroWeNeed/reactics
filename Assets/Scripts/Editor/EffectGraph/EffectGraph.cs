using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Reactics.Battle;
using Reactics.Battle.Map;
using Reactics.Commons;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using System.Text;

namespace Reactics.Editor
{
    public class EffectGraph : ScriptableObject
    {
        public static readonly Guid MasterNodeId = Guid.Parse("b3f706b2-b465-460d-bc2e-cc5b28e13803");
        public Dictionary<Guid, Entry> entries = new Dictionary<Guid, Entry>();

        private List<Type> effectTypes = new List<Type>();
        public ReadOnlyCollection<Type> EffectTypes { get; private set; }

        private void Awake()
        {
            EffectTypes = PopulateTypeList(effectTypes).AsReadOnly();
        }
        private List<Type> PopulateTypeList(List<Type> typeList = null)
        {
            List<Type> validTypes = typeList;
            if (validTypes == null)
                validTypes = new List<Type>();
            else
                validTypes.Clear();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    if (IsValidType(type))
                        validTypes.Add(type);
            return validTypes;
        }
        public static bool IsValidType(Type type)
        {
            return type.IsUnmanaged() && typeof(IEffect).IsAssignableFrom(type) && !typeof(IUtilityEffect).IsAssignableFrom(type);
        }


        public class Entry
        {
            public readonly Guid Id;
            public Guid NextId = Guid.Empty;
            public Type Type;
            public readonly Dictionary<string, object> Values = new Dictionary<string, object>();

            public Entry(object source) : this(Guid.NewGuid(), source) { }
            public Entry(Guid id, object source) : this(id, source, Array.Empty<Guid>()) { }
            public Entry(object source, Guid[] nodes) : this(Guid.NewGuid(), source, nodes) { }
            public Entry(Guid id, object source, Guid[] nodes)
            {
                Id = id;
                Type = source.GetType();

                object value;
                foreach (var field in source.GetType().GetFields())
                {
                    if (IsNodeReference(field, out Type nodeType))
                    {
                        var index = (int)field.GetValue(source);
                        value = new NodeIndex(nodeType, index >= 0 && index < nodes.Length ? nodes[index] : Guid.Empty);
                    }
                    else if (IsConfigurable(field))
                        value = field.GetValue(source);
                    else
                        continue;
                    Values[field.Name] = value;
                }
            }
            public Entry(Type type = null) : this(Guid.NewGuid(), type) { }
            public Entry(Guid id, Type type = null)
            {
                Id = id;
                Type = type;
                if (type != null)
                {
                    object value;
                    foreach (var field in type.GetFields())
                    {
                        if (IsNodeReference(field, out Type nodeType))
                            value = new NodeIndex(nodeType);
                        else if (IsConfigurable(field))
                            value = field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : default;
                        else
                            continue;
                        Values[field.Name] = value;
                    }
                }
            }
            public Entry(Guid id, Dictionary<string, object> values, Type type) : this(id, values, Array.Empty<Guid>(), type) { }
            public Entry(Guid id, Dictionary<string, object> values, Guid[] nodes, Type type)
            {
                Id = id;
                Type = type;
                if (type != null)
                {
                    object value;
                    foreach (var field in type.GetFields())
                    {
                        if (IsNodeReference(field, out Type nodeType))
                            value = new NodeIndex(nodeType, values.TryGetValue(field.Name, out object index) && index is int intIndex && intIndex >= 0 && intIndex < nodes.Length ? nodes[intIndex] : Guid.Empty);
                        else if (IsConfigurable(field))
                            value = values.TryGetValue(field.Name, out object result) ? result : field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : default;
                        else
                            continue;
                        Values[field.Name] = value;
                    }
                }
            }

            public object this[string key]
            {
                get => Values[key];
                set => Values[key] = value;
            }
            private bool IsNodeReference(FieldInfo field, out Type nodeType)
            {
                SerializeNodeIndex attr = field.GetCustomAttribute<SerializeNodeIndex>();
                if (attr != null && typeof(int).IsAssignableFrom(field.FieldType))
                {
                    nodeType = attr.nodeType;
                    return true;
                }
                else
                {
                    nodeType = null;
                    return false;
                }
            }
            private bool IsConfigurable(FieldInfo field) => field.FieldType.IsSerializable && (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null) && field.GetCustomAttribute<HideInInspector>() == null;

            public override string ToString()
            {
                return $"Entry(Id: {Id}, NextId: {NextId}";
            }
        }


    }
    public static class EffectGraphController
    {
        public const string InputPortClassName = "input-port";
        public const string OutputPortClassName = "output-port";
        public const string ConfigurableFieldClassName = "config-field";
        public const string EffectGraphNodeClassName = "effect-graph-node";
        public const string EffectGraphMasterNodeClassName = "effect-graph-master-node";
        public static readonly Type[] SupportedPortTypes = { typeof(Point), typeof(MapBodyDirection), typeof(MapBodyTarget) };
        private static readonly Dictionary<Type, Color> portColors = new Dictionary<Type, Color>();
        public static readonly ReadOnlyDictionary<Type, Color> PortColors = new ReadOnlyDictionary<Type, Color>(portColors);
        static EffectGraphController()
        {
            portColors[typeof(Point)] = Color.cyan;
            portColors[typeof(MapBodyTarget)] = Color.yellow;
            portColors[typeof(MapBodyDirection)] = Color.magenta;
        }
        public static Node CreateNode(GraphView graphView, EffectGraph effectGraph, object source, Rect layout) => CreateNode(graphView, effectGraph, new EffectGraph.Entry(Guid.NewGuid(), source), layout);
        public static Node CreateNode(GraphView graphView, EffectGraph effectGraph, Type type, Rect layout) => CreateNode(graphView, effectGraph, new EffectGraph.Entry(Guid.NewGuid(), type), layout);
        public static Node CreateNode(GraphView graphView, EffectGraph effectGraph, EffectGraph.Entry entry, Rect layout)
        {

            var node = new Node();
            //Undo.RecordObject(effectGraph, $"Create Entry ({entry.Id}, {entry.Type})");
            effectGraph.entries[entry.Id] = entry;
            graphView.AddElement(node);
            node.SetPosition(layout);
            ConstructNodeHeader(node, effectGraph, entry);
            ConstructNodeOutputPort(node, graphView, entry);
            ConstructNode(node, entry);

            return node;
        }
        public static void CreateNodes(GraphView graphView, EffectGraph effectGraph, EffectGraph.Entry[] entries, Rect[] layouts)
        {
            var nodes = new Node[entries.Length];
            var undoName = entries.Aggregate(new StringBuilder(), (acc, entry) => acc.Append($"{(acc.Length > 0 ? ", " : "")}({entry.Id},{entry.Type})")).ToString();

            //Undo.RecordObject(effectGraph, $"Create Entries {undoName}");
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i] = new Node();
                effectGraph.entries[entries[i].Id] = entries[i];
                graphView.AddElement(nodes[i]);
                if (layouts != null)
                    nodes[i].SetPosition(layouts[i]);
                nodes[i].viewDataKey = entries[i].Id.ToString();
                ConstructNodeHeader(nodes[i], effectGraph, entries[i]);
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                ConstructNodeOutputPort(nodes[i], graphView, entries[i]);
                ConstructNode(nodes[i], entries[i]);
            }
        }

        public static void AdjustConnections(EffectGraph.Entry[] entries, Guid[] oldIds)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].NextId != Guid.Empty && entries[i].NextId != EffectGraph.MasterNodeId)
                {
                    var newIndex = Array.FindIndex(oldIds, (x) => x.Equals(entries[i].NextId));
                    if (newIndex >= 0)
                    {
                        entries[i].NextId = entries[newIndex].Id;
                    }
                }
                foreach (var key in entries[i].Values.Keys)
                {
                    if (entries[i].Values[key] is NodeIndex nodeIndex)
                    {
                        if (nodeIndex.node != Guid.Empty && nodeIndex.node != EffectGraph.MasterNodeId)
                        {
                            var newIndex = Array.FindIndex(oldIds, (x) => x.Equals(nodeIndex.node));
                            if (newIndex >= 0)
                            {
                                entries[i].Values[key] = new NodeIndex(nodeIndex.type, entries[newIndex].Id);
                            }
                        }
                    }

                }

            }
        }
        private static void ConstructNodeHeader(Node node, EffectGraph effectGraph, EffectGraph.Entry entry)
        {
            node.viewDataKey = entry.Id.ToString();
            node.title = entry.Type.Name;
            node.AddToClassList(EffectGraphNodeClassName);
            node.userData = effectGraph.GetInstanceID();
            var inputPort = CreatePort(GetNodeType(entry.Type), "In", Direction.Input, Port.Capacity.Multi);
            inputPort.AddToClassList(InputPortClassName);
            node.inputContainer.Add(inputPort);

            node.RegisterCallback<ChangeFieldEvent>((evt) =>
            {
                var evtNode = evt.currentTarget as Node;
                var evtId = Guid.Parse(evtNode.viewDataKey);
                var target = EditorUtility.InstanceIDToObject((int)(evt.currentTarget as VisualElement).userData) as EffectGraph;
                //Undo.RecordObject(target, $"Set Entry({evtId}) Field {evt.name} to {evt.value}");
                target.entries[evtId][evt.name] = evt.value;
            });
            node.RegisterCallback<DetachFromPanelEvent>((evt) =>
            {


                var self = evt.target as Node;
                if (Guid.TryParse(self.viewDataKey, out Guid id))
                {
                    var target = EditorUtility.InstanceIDToObject((int)self.userData) as EffectGraph;
                    if (target != null)
                    {
                        if (target.entries.ContainsKey(id))
                        {
                            var oldEntry = target.entries[id];
                            //Undo.RecordObject(target, $"Delete Entry({id},{oldEntry.Type})");
                            target.entries.Remove(id);
                        }

                    }
                }
            });
        }

        private static void ConstructNodeOutputPort(Node node, GraphView graphView, EffectGraph.Entry entry)
        {
            var outputPort = new EffectGraphPort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, GetNodeType(entry.Type))
            {
                portColor = portColors[GetNodeType(entry.Type)],
                portName = "Out"
            };
            outputPort.AddToClassList(OutputPortClassName);
            outputPort.RegisterCallback<PortChangedEvent>(PortChangedOutputCallback);
            node.outputContainer.Add(outputPort);
            if (entry.NextId != Guid.Empty)
            {
                var port = graphView.GetElementByGuid(entry.NextId.ToString())?.Q<Port>(null, InputPortClassName);
                if (port != null)
                {
                    graphView.AddElement(outputPort.ConnectTo(port));
                }
            }
        }

        public static void PortChangedOutputCallback(PortChangedEvent evt)
        {
            var edge = evt.edges.FirstOrDefault();
            var evtNode = (evt.target as Port).node;
            if (Guid.TryParse(evtNode.viewDataKey, out Guid nodeId))
            {
                if (edge != null && edge.input != null)
                {
                    if (Guid.TryParse(edge.input.node.viewDataKey, out Guid targetId))
                        (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = targetId;
                    else if (edge.input.node != null && edge.input.node.ClassListContains(EffectGraphMasterNodeClassName))
                        (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = EffectGraph.MasterNodeId;
                    else
                        (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = Guid.Empty;
                }
                else
                {
                    (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = Guid.Empty;
                }
            }
        }
        public static void PortChangedFieldCallback(PortChangedEvent evt)
        {
            var edge = evt.edges.FirstOrDefault();
            var evtNode = (evt.target as Port).node;
            if (Guid.TryParse(evtNode.viewDataKey, out Guid nodeId))
            {
                if (edge != null && edge.input != null)
                {
                    if (Guid.TryParse(edge.input.node.viewDataKey, out Guid targetId))
                        (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = targetId;
                    else if (edge.input.node != null && edge.input.node.ClassListContains(EffectGraphMasterNodeClassName))
                        (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = EffectGraph.MasterNodeId;
                    else
                        (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = Guid.Empty;
                }
                else
                {
                    (EditorUtility.InstanceIDToObject((int)evtNode.userData) as EffectGraph).entries[nodeId].NextId = Guid.Empty;
                }
            }
        }
        private static void ConstructNode(Node node, EffectGraph.Entry entry)
        {
            node.Query<VisualElement>(null, ConfigurableFieldClassName).ToList().ForEach((element) => element.RemoveFromHierarchy());
            var callback = GetRegisterCallback();
            foreach (var kv in entry.Values)
            {

                var entryValueType = kv.Value.GetType();
                var element = VisualElementProviders.Create(entryValueType, kv.Key, kv.Value);
                element.viewDataKey = kv.Key;
                element.AddToClassList(ConfigurableFieldClassName);
                var changeEvent = typeof(ChangeEvent<>).MakeGenericType(entryValueType);
                var changeEventCallbackMethod = typeof(EffectGraphController).GetMethod("UpdateEffectGraph", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(entryValueType);

                var changeEventCallback = Delegate.CreateDelegate(typeof(EventCallback<>).MakeGenericType(changeEvent), changeEventCallbackMethod);
                callback.MakeGenericMethod(changeEvent).Invoke(element, new object[] { changeEventCallback, TrickleDown.NoTrickleDown });
                if (element.GetType().GetCustomAttribute<OutputContainerElement>() != null)
                    node.outputContainer.Add(element);
                else
                    node.inputContainer.Add(element);
            }
        }
        private static MethodInfo GetRegisterCallback()
        {
            return typeof(CallbackEventHandler).GetMethods().Where((method) => method.Name == "RegisterCallback" && method.GetGenericArguments().Length == 1).FirstOrDefault();
        }
        private static void UpdateEffectGraph<T>(ChangeEvent<T> evt)
        {
            var target = evt.target as VisualElement;
            
            using (ChangeFieldEvent fieldEvt = ChangeFieldEvent.GetPooled(typeof(T), target.viewDataKey, evt.newValue))
            {
                fieldEvt.target = evt.target;
                target.SendEvent(fieldEvt);
            }
        }
        private static Port CreatePort(Type type, string name, Direction direction, Port.Capacity capacity)
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, direction, capacity, type);
            port.portColor = portColors[type];
            port.portName = name;
            return port;
        }
        public static Type GetNodeType(Type type)
        {
            if (typeof(IEffect).IsAssignableFrom(type))
                return GetNodeType(type.GetInterfaces().First((x) => x.IsGenericType && x.GetGenericTypeDefinition().Equals(typeof(IEffect<>))).GetGenericArguments()[0]);
            if (SupportedPortTypes.Contains(type))
                return type;
            else
                throw new ArgumentException($"Unsupported Type", "type");
        }
        public static string ToJson(IEnumerable<Guid> ids, EffectGraph effectGraph)
        {
            return JObject.FromObject(new
            {
                effects = ids.Select((id) => effectGraph.entries[id]).ToArray()
            }).ToString(Formatting.None, new EffectGraphEntryConverter());
        }
        /* 
                public static void RefreshValues(EffectGraph effectGraph, GraphView graphView, Action<Port, GraphView> connectToMaster)
                {
                    Debug.Log("Refreshing...");
                    var targets = new Dictionary<Guid, Node>();
                    graphView.Query<Node>(null, EffectGraphNodeClassName).ForEach((node) =>
                    {
                        if (Guid.TryParse(node.viewDataKey, out Guid id))
                            targets[id] = node;
                    });
                    Debug.Log(effectGraph.entries.Count);
                    foreach (var kv in effectGraph.entries)
                    {

                        if (targets.ContainsKey(kv.Key))
                        {
                            Debug.Log("HERE");
                            targets[kv.Key].Query<GraphField>(null, ConfigurableFieldClassName).ForEach((field) =>
                                                                {
                                                                    Debug.Log(field.viewDataKey);
                                                                    if (kv.Value.Values.TryGetValue(field.viewDataKey, out object v))
                                                                    {

                                                                        field.TrySetValueWithoutNotify(v);
                                                                        Debug.Log("SETTING");
                                                                    }

                                                                });

                            targets.Remove(kv.Key);
                        }
                        else
                        {
                            CreateNode(graphView, effectGraph, kv.Value, new Rect(100, 100, 100, 100));
                        }
                    }
                    foreach (var key in targets.Keys)
                    {
                        targets[key].RemoveFromHierarchy();

                    }


                }
                 */
        public static void FromJson(string json, out EffectGraph.Entry[] newEntries, out Guid[] oldIds) => FromJson(JObject.Parse(json), out newEntries, out oldIds);

        public static void CopyEntries(EffectGraph.Entry[] jsonEntries, out EffectGraph.Entry[] newEntries, out Guid[] oldIds)
        {
            newEntries = new EffectGraph.Entry[jsonEntries.Length];
            oldIds = new Guid[jsonEntries.Length];

            for (int i = 0; i < newEntries.Length; i++)
            {
                newEntries[i] = new EffectGraph.Entry(Guid.NewGuid(), jsonEntries[i].Type)
                {
                    NextId = jsonEntries[i].NextId
                };
                oldIds[i] = jsonEntries[i].Id;
            }
            for (int i = 0; i < newEntries.Length; i++)
            {
                if (jsonEntries[i].NextId != Guid.Empty)
                {
                    for (int j = 0; j < jsonEntries.Length; j++)
                    {
                        if (jsonEntries[i].NextId.Equals(jsonEntries[j].Id))
                        {
                            newEntries[i].NextId = jsonEntries[j].Id;
                            break;
                        }
                    }
                }
                foreach (var key in jsonEntries[i].Values.Keys)
                {
                    newEntries[i][key] = jsonEntries[i][key];
                    if (jsonEntries[i][key] is NodeIndex nodeIndex && nodeIndex.node != Guid.Empty)
                    {
                        for (int j = 0; j < jsonEntries.Length; j++)
                        {
                            if (nodeIndex.node.Equals(jsonEntries[j].Id))
                            {
                                newEntries[i][key] = new NodeIndex(nodeIndex.type, jsonEntries[j].Id);
                                break;
                            }
                        }
                    }

                }
            }

        }
        public static void FromJson(JObject jsonObject, out EffectGraph.Entry[] newEntries, out Guid[] oldIds)
        {
            CopyEntries(jsonObject["effects"].Values<EffectGraph.Entry>().ToArray(), out newEntries, out oldIds);
        }

        public static void SerializeTo(EffectGraph effectGraph, EffectAsset effectAsset, GraphNodeLayout layoutAsset, Node[] initialNodes, INodeReader nodeReader)
        {

            var effectSerializedObject = new SerializedObject(effectAsset);
            var layoutSerializedObject = new SerializedObject(layoutAsset);
            var nodes = CollectNodes(initialNodes, nodeReader).Where((z) => z.viewDataKey != EffectGraph.MasterNodeId.ToString()).Select((x) => new
            {
                node = x,
                id = Guid.TryParse(x.viewDataKey, out Guid y) ? y : Guid.Empty,
                entry = y != Guid.Empty ? effectGraph.entries[y] : null
            }).Where((x) => x.id != Guid.Empty).ToArray();

            var effectProperty = effectSerializedObject.FindProperty("effect");
            var typeProperty = effectSerializedObject.FindProperty("type");
            var rootIndicesProperty = effectSerializedObject.FindProperty("rootIndices");
            var positions = layoutSerializedObject.FindProperty("positions");

            effectProperty.arraySize = nodes.Length;
            positions.arraySize = nodes.Length;
            var roots = new List<Tuple<int, Type>>();
            List<IEffect> additions = new List<IEffect>();
            int index;

            for (index = 0; index < nodes.Length; index++)
            {
                var property = effectProperty.GetArrayElementAtIndex(index);
                positions.GetArrayElementAtIndex(index).rectValue = nodes[index].node.GetPosition();

                var effect = Activator.CreateInstance(nodes[index].entry.Type) as IEffect;
                var values = nodes[index].entry.Values.GetEnumerator();
                while (values.MoveNext())
                {
                    var kv = values.Current;
                    if (kv.Value is NodeIndex nIndex)
                        nodes[index].entry.Type.GetField(kv.Key).SetValue(effect, Array.FindIndex(nodes, (n) => n.id.Equals(nIndex.node)));
                    else
                        nodes[index].entry.Type.GetField(kv.Key).SetValue(effect, kv.Value);
                }

                if (nodeReader.IsRoot(nodes[index].node))
                    roots.Add(Tuple.Create(index, nodeReader.GetNodeType(nodes[index].node)));

                property.managedReferenceValue = effect;
                if (nodes[index].entry.NextId != Guid.Empty)
                {
                    var linearEffect = Activator.CreateInstance<LinearEffect>();
                    linearEffect.effect = nodes.Length + additions.Count;
                    if (nodes[index].entry.NextId == EffectGraph.MasterNodeId)
                        linearEffect.next = -2;
                    else
                        linearEffect.next = Array.FindIndex(nodes, (n) => n.id.Equals(nodes[index].entry.NextId));
                    additions.Add(effect);
                    property.managedReferenceValue = linearEffect;
                }
            }
            effectProperty.arraySize = nodes.Length + additions.Count;
            while (index < nodes.Length + additions.Count)
            {
                effectProperty.GetArrayElementAtIndex(index).managedReferenceValue = additions[index - nodes.Length];
                index++;
            }
            rootIndicesProperty.arraySize = roots.Count;
            Type targetType = null;
            for (int i = 0; i < roots.Count; i++)
            {
                if (targetType == null)
                    targetType = roots[i].Item2;
                else if (!targetType.Equals(roots[i].Item2))
                {
                    throw new InvalidOperationException("Root nodes must all be the same type");
                }
                rootIndicesProperty.GetArrayElementAtIndex(i).intValue = roots[i].Item1;
            }
            typeProperty.enumValueIndex = Array.IndexOf(Enum.GetValues(typeof(TargetType)), TargetTypeUtility.GetType(targetType));
            layoutSerializedObject.ApplyModifiedProperties();
            effectSerializedObject.ApplyModifiedProperties();
            layoutSerializedObject.Update();
            effectSerializedObject.Update();
        }

        public static void DeserializeFrom(EffectGraph effectGraph, GraphView graphView, SerializedObject effectSerializedObject, SerializedObject layoutSerializedObject, Action<Port, GraphView> connectToMaster)
        {


            EffectAsset asset = (EffectAsset)effectSerializedObject.targetObject;
            var guids = new Guid[asset.EffectCount];
            var entries = new Dictionary<Guid, EffectGraph.Entry>();
            var masterConnections = new List<Guid>();
            for (int i = 0; i < asset.EffectCount; i++)
            {
                guids[i] = Guid.NewGuid();
            }
            for (int i = 0; i < asset.EffectCount; i++)
            {
                if (entries.ContainsKey(guids[i]))
                    continue;
                if (asset.effect[i] is LinearEffect linearEffect)
                {
                    if (linearEffect.next == -2)
                    {

                        entries[guids[linearEffect.effect]] = new EffectGraph.Entry(guids[linearEffect.effect], asset.effect[linearEffect.effect], guids)
                        {
                            NextId = EffectGraph.MasterNodeId
                        };
                        masterConnections.Add(guids[linearEffect.effect]);
                    }
                    else if (linearEffect.next >= 0)
                    {
                        entries[guids[linearEffect.effect]] = new EffectGraph.Entry(guids[linearEffect.effect], asset.effect[linearEffect.effect], guids)
                        {
                            NextId = guids[linearEffect.next]
                        };
                    }
                    guids[i] = guids[linearEffect.effect];
                }
            }
            for (int i = 0; i < asset.EffectCount; i++)
            {
                if (entries.ContainsKey(guids[i]))
                    continue;
                if (!(asset.effect[i] is LinearEffect))
                {
                    entries[guids[i]] = new EffectGraph.Entry(guids[i], asset.effect[i], guids);
                }
            }
            var normalizedEntries = entries.Values.ToArray();
            Rect[] layouts = new Rect[normalizedEntries.Length];
            if (layoutSerializedObject != null)
            {
                for (int i = 0; i < layoutSerializedObject.FindProperty("positions").arraySize; i++)
                {
                    layouts[i] = layoutSerializedObject.FindProperty("positions").GetArrayElementAtIndex(i).rectValue;
                }
            }
            else
            {
                layouts = null;
            }
            CreateNodes(graphView, effectGraph, entries.Values.ToArray(), layouts);
            foreach (var connection in masterConnections)
            {
                var port = graphView.GetElementByGuid(connection.ToString())?.Q<Port>(null, OutputPortClassName);
                connectToMaster(port, graphView);
            }
        }
        private static List<Node> CollectNodes(Node[] nodes, INodeReader nodeReader)
        {
            var processedNodes = new List<Node>();
            return CollectNodes(nodes, nodeReader, processedNodes);
        }
        private static List<Node> CollectNodes(Node[] nodes, INodeReader nodeReader, List<Node> processedNodes)
        {
            foreach (var node in nodes)
            {
                if (!processedNodes.Contains(node))
                {
                    processedNodes.Add(node);
                }
                foreach (var foundNode in nodeReader.Collect(node))
                {
                    if (!processedNodes.Contains(foundNode))
                    {
                        processedNodes.Add(foundNode);
                        CollectNodes(foundNode, nodeReader, processedNodes);
                    }
                }
            }
            return processedNodes;
        }

        private static List<Node> CollectNodes(Node node, INodeReader nodeReader, List<Node> processedNodes)
        {
            if (!processedNodes.Contains(node))
            {
                processedNodes.Add(node);
            }
            foreach (var foundNode in nodeReader.Collect(node))
            {
                if (!processedNodes.Contains(foundNode))
                {
                    processedNodes.Add(foundNode);
                    CollectNodes(foundNode, nodeReader, processedNodes);
                }
            }
            return processedNodes;
        }

        /*         public static EffectGraph DeserializeFrom(Guid assetGuid)
                {

                } */
    }
}