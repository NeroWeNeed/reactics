using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Reactics.Editor.Graph {
    public class UnityObjectGraphSequenceSerializer : ObjectGraphSerializer<SerializedObject> {

        public string dataPropertyPath;
        public string variablePropertyPath;
        public string layoutPropertyPath = "layout";

        public override bool CanSerialize(IObjectGraphNodeProvider provider, ObjectGraphView graphView, out string message) {
            var roots = provider.CollectRoots(graphView);

            if (roots == null || roots.Length == 1) {
                message = string.Empty;
                return true;
            }
            else {
                message = "Only 1 root is allowed.";
                return false;
            }

        }
        public override bool Deserialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out SerializedObject result) {
            var targetObject = target?.targetObject;
            if (targetObject == null) {
                result = null;
                return true;
            }
            var layout = UnityObjectGraphSerializer.GetNodeLayoutObject(dataPropertyPath, AssetDatabase.GetAssetPath(targetObject), false)?.FindProperty(layoutPropertyPath);
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
                graphView.Model.SetEntry(nodes[index].Id, kv.Value);
                graphView.AddElement(nodes[index]);
                index++;
            }


            foreach (var node in nodes) {
                node.Refresh();

            }
            graphView.Validate();
            result = target;
            return true;

        }
        public static SortedDictionary<string, ObjectGraphModel.NodeEntry> DeserializeEntries(object[] data, ObjectGraphView graphView) {
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

            var result = new SortedDictionary<string, ObjectGraphModel.NodeEntry>();
            for (int i = 0; i < payload.entries.Count; i++) {
                result[payload.entries[i].key] = new ObjectGraphModel.NodeEntry(payload.entries[i].data, i + 1 < payload.entries.Count ? ids[i + 1] : graphView.MasterNode.viewDataKey, payload);
            }
            return result;

        }
        public static ObjectGraphNode[] CollectFromRoot(ObjectGraphNode root) {
            List<ObjectGraphNode> result = new List<ObjectGraphNode>();
            var current = root;

            while (current != null) {
                result.Add(current);
                current = current.output.connections.FirstOrDefault()?.input?.node as ObjectGraphNode;
            }
            return result.ToArray();
        }
        public override bool Serialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out SerializedObject result) {
            var dataProperty = target.FindProperty(dataPropertyPath);
            if (!dataProperty.isArray) {
                throw new ArgumentException("Property isn't an array of managed references");
            }
            var root = Array.Find(provider.CollectNodes(graphView), (n) => n.IsRoot);
            ObjectGraphNode[] nodes = CollectFromRoot(root);
            var layout = UnityObjectGraphSerializer.GetNodeLayoutObject(dataProperty);
            var payload = UnityObjectGraphSerializer.Serialize(target, provider, graphView, nodes);
            dataProperty.arraySize = payload.entries.Count;
            var layoutProp = layout.FindProperty(layoutPropertyPath);
            layoutProp.arraySize = payload.entries.Count((entry) => entry.node != null);
            for (int i = 0; i < dataProperty.arraySize; i++) {
                dataProperty.GetArrayElementAtIndex(i).managedReferenceValue = payload.entries[i].data;
                if (i < layoutProp.arraySize)
                    layoutProp.GetArrayElementAtIndex(i).rectValue = payload.entries[i].node.GetPosition();
            }
            if (provider is IObjectGraphPostSerializerCallback callback) {
                callback.OnPostSerialize(target, ref payload);
            }
            target.ApplyModifiedProperties();
            layout.ApplyModifiedProperties();
            result = target;
            return true;
        }
    }
}