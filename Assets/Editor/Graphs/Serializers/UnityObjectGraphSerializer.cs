using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactics.Core.Commons;
using UnityEditor;
using UnityEngine;

namespace Reactics.Core.Editor.Graph {

    public class UnityObjectGraphSerializer : ObjectGraphSerializer<SerializedObject> {

        public string dataPropertyPath;
        public string rootPropertyPath;
        public string variablePropertyPath;

        public string layoutPropertyPath = "layout";

        public override bool CanSerialize(IObjectGraphNodeProvider provider, ObjectGraphView graphView, out string message) {
            message = string.Empty;
            return true;
        }

        public override bool Deserialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out SerializedObject result) {
            return Deserialize(target, provider, graphView, dataPropertyPath, layoutPropertyPath, out result);
        }
        public static bool Deserialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, string dataPropertyPath, string layoutPropertyPath, out SerializedObject result) {
            var targetObject = target?.targetObject;
            if (targetObject == null) {
                result = null;
                return true;
            }
            var layout = GetNodeLayoutObject(dataPropertyPath, AssetDatabase.GetAssetPath(targetObject), false)?.FindProperty(layoutPropertyPath);
            var dataField = targetObject.GetType().GetField(dataPropertyPath);
            if (dataField?.FieldType.IsArray != true) {
                Debug.LogError("Invalid Data Field");
                result = target;
                return false;
            }
            var data = (object[])dataField.GetValue(targetObject);
            if (provider is IObjectGraphPreDeserializerCallback callback) {
                data = callback.OnPreDeserialize(target, graphView, data);
            }
            if (data == null) {
                result = target;
                return true;
            }
            var entries = DeserializeEntries(data, graphView);
            if (entries == null) {
                result = target;
                return true;
            }
            if (provider is IObjectGraphPostDeserializerCallback callback2) {
                callback2.OnPostDeserialize(target, graphView, ref entries);
            }
            var nodes = new ObjectGraphNode[entries.Count];
            int index = 0;
            foreach (var kv in entries) {
                nodes[index] = provider.Create(kv.Key, kv.Value, layout == null ? default : layout.GetArrayElementAtIndex(index).rectValue);
                graphView.ModelEditor.SetEntry(nodes[index], kv.Value);
                nodes[index].ModelEditor = graphView.ModelEditor;
                graphView.AddElement(nodes[index]);
                index++;
            }

            foreach (var node in nodes) {
                node.Entry = entries[node.viewDataKey];
                node.SyncWithEntry();
            }
            graphView.Validate();
            result = target;
            return true;

        }
        public static SortedDictionary<string, ObjectGraphModel.Entry> DeserializeEntries(object[] data, ObjectGraphView graphView) {
            var ids = new string[data.Length];
            var entries = new ObjectGraphSerializerPayload.Entry[data.Length];
            for (int i = 0; i < data.Length; i++) {
                ids[i] = Guid.NewGuid().ToString();
            }

            Array.Sort(ids);
            for (int i = 0; i < data.Length; i++) {
                entries[i] = new ObjectGraphSerializerPayload.Entry
                {
                    key = ids[i],
                    data = data[i]
                };
            }
            var payload = new ObjectGraphSerializerPayload
            {
                graphView = graphView,
                entries = entries.ToList()
            };

            var result = new SortedDictionary<string, ObjectGraphModel.Entry>();
            for (int i = 0; i < payload.entries.Count; i++) {
                result[payload.entries[i].key] = ObjectGraphModel.Entry.Create(payload.entries[i].data, null, payload);
            }
            return result;
        }


        public override bool Serialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out SerializedObject result) {
            var rootProperty = target.FindProperty(rootPropertyPath);
            var dataProperty = target.FindProperty(dataPropertyPath);
            if (!dataProperty.isArray || !rootProperty.isArray) {
                throw new ArgumentException("Property isn't an array of managed references");
            }
            ObjectGraphNode[] nodes = provider.CollectNodes(graphView);
            var rootNodes = nodes.Where((n) => n.IsRoot).ToArray();
            var roots = nodes.Select((_, i) => i).Where((i) => nodes[i].IsRoot).ToArray();
            var layout = GetNodeLayoutObject(dataProperty);

            var payload = Serialize(target, provider, graphView, nodes);
            if (provider is IObjectGraphPreSerializerCallback callback) {
                callback.OnPreSerialize(target, ref payload);
            }
            dataProperty.arraySize = payload.entries.Count;
            var layoutProp = layout.FindProperty(layoutPropertyPath);
            layoutProp.arraySize = payload.entries.Count((entry) => entry.node != null);

            for (int i = 0; i < dataProperty.arraySize; i++) {
                dataProperty.GetArrayElementAtIndex(i).managedReferenceValue = payload.entries[i].data;
                if (i < layoutProp.arraySize)
                    layoutProp.GetArrayElementAtIndex(i).rectValue = payload.entries[i].node.GetPosition();
            }
            rootProperty.arraySize = roots.Length;
            for (int i = 0; i < rootProperty.arraySize; i++) {
                rootProperty.GetArrayElementAtIndex(i).intValue = roots[i];
            }
            if (provider is IObjectGraphPostSerializerCallback callback2) {
                callback2.OnPostSerialize(target, ref payload);
            }
            target.ApplyModifiedProperties();
            layout.ApplyModifiedProperties();

            result = target;
            return true;


        }
        public static ObjectGraphSerializerPayload Serialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, ObjectGraphNode[] nodes) {
            var payload = new ObjectGraphSerializerPayload
            {
                graphView = graphView,
                entries = nodes.Where((x) => graphView.Model.entries.Keys.Any((y) => y == x.viewDataKey)).Select((z) => new ObjectGraphSerializerPayload.Entry
                {
                    key = z.viewDataKey
                }).ToList()
            };

            for (int i = 0; i < payload.entries.Count; i++) {
                var obj = Activator.CreateInstance(graphView.Model.entries[payload.entries[i].key].type);
                foreach (var kv in graphView.Model.entries[payload.entries[i].key].values) {
                    var fieldInfo = graphView.Model.entries[payload.entries[i].key].type.GetField(kv.Key);
                    var fieldValue = kv.Value;
                    ObjectGraphNodeValueConverters.TryToConvertToOriginal(kv.Value, payload, out fieldValue);
                    fieldInfo.SetValue(obj, fieldValue);
                }
                payload.entries[i] = new ObjectGraphSerializerPayload.Entry(payload.entries[i], obj, graphView.Model.entries[payload.entries[i].key], nodes.First((x) => x.viewDataKey == payload.entries[i].key));

            }

            return payload;
        }

        public static SerializedObject GetNodeLayoutObject(SerializedProperty property) {
            return GetNodeLayoutObject(property.name, AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
        }
        public static SerializedObject GetNodeLayoutObject(string name, string mainAssetPath, bool create = true) {
            foreach (var item in AssetDatabase.LoadAllAssetRepresentationsAtPath(mainAssetPath)) {
                if (item.name == $"{name}_node_layout" && item is ObjectGraphNodeLayout) {
                    return new SerializedObject(item);
                }
            }
            if (create) {
                UnityEngine.Object layout = ScriptableObject.CreateInstance<ObjectGraphNodeLayout>();
                layout.name = $"{name}_node_layout";
                AssetDatabase.AddObjectToAsset(layout, mainAssetPath);
                return new SerializedObject(layout);
            }
            else {
                return null;
            }
        }

    }
}