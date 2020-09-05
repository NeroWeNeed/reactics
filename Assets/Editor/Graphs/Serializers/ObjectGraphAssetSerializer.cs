using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ObjectGraphAssetSerializer<TObject, TOutputAsset> : ObjectGraphSerializer<SerializedObject> where TOutputAsset : ScriptableObject {
        public override bool Deserialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out SerializedObject result) {
            var targetObject = target?.targetObject;
            if (targetObject == null) {
                result = null;
                return true;
            }
            var nodesProperty = target.FindProperty("nodes");
            //var objectsProperty = target.FindProperty("objects");
            var variablesProperty = target.FindProperty("variables");
            var nodes = new ObjectGraphNode[nodesProperty.arraySize];
            var variables = new ObjectGraphVariableNode[variablesProperty.arraySize];
            var asset = (ObjectGraphAsset<TObject, TOutputAsset>)targetObject;
            for (int i = 0; i < nodes.Length; i++) {
                var nodeProperty = nodesProperty.GetArrayElementAtIndex(i);
                var obj = asset.Objects[nodeProperty.FindPropertyRelative("index").intValue];
                graphView.Model.CreateEntry(obj.GetType(), nodeProperty.FindPropertyRelative("id").stringValue);
                nodes[i] = provider.Create(nodeProperty.FindPropertyRelative("id").stringValue, obj.GetType(), nodeProperty.FindPropertyRelative("layout").rectValue);
                graphView.AddElement(nodes[i]);
            }
            for (int i = 0; i < nodes.Length; i++) {
                var nodeProperty = nodesProperty.GetArrayElementAtIndex(i);
                var obj = asset.Objects[nodeProperty.FindPropertyRelative("index").intValue];
                var nextIndex = nodeProperty.FindPropertyRelative("next").intValue;
                string next = null;
                if (nextIndex >= 0 && nextIndex < nodes.Length) {
                    next = nodes[nextIndex].viewDataKey;
                }
                else if (nextIndex == -2) {
                    next = graphView.MasterNode.viewDataKey;
                }
                graphView.Model.SetEntryData(nodes[i].viewDataKey, obj, nodes, next);
                nodes[i].GraphView = graphView;
                nodes[i].Refresh();
            }
            for (int i = 0; i < variables.Length; i++) {
                var variableProperty = variablesProperty.GetArrayElementAtIndex(i);
                var variable = graphView.Model.GetVariable(variableProperty.FindPropertyRelative("type").stringValue, variableProperty.FindPropertyRelative("address").stringValue);
                if (variable == null)
                    continue;
                variables[i] = new ObjectGraphVariableNode(variable, variableProperty.FindPropertyRelative("id").stringValue, variableProperty.FindPropertyRelative("layout").rectValue);

                var variableConnectionsProperty = variableProperty.FindPropertyRelative("connections");
                var variableTargets = new ObjectGraphModel.VariableEntry.Target[variableConnectionsProperty.arraySize];
                for (int j = 0; j < variableConnectionsProperty.arraySize; j++) {
                    var variableConnectionProperty = variableConnectionsProperty.GetArrayElementAtIndex(j);
                    var index = variableConnectionProperty.FindPropertyRelative("index").intValue;
                    var field = variableConnectionProperty.FindPropertyRelative("field").stringValue;
                    if (index < 0 || index >= nodes.Length || !graphView.Model.entries[nodes[index].viewDataKey].values.ContainsKey(field))
                        continue;
                    Debug.Log($"ID: {nodes[index].viewDataKey}, FIELD: {field}");
                    variableTargets[j] = new ObjectGraphModel.VariableEntry.Target
                    {
                        id = nodes[index].viewDataKey,
                        field = field
                    };
                }
                graphView.Model.CreateVariableEntry(variable, variableProperty.FindPropertyRelative("id").stringValue, variableTargets);
                graphView.AddElement(variables[i]);
                //variables[i].Refresh(graphView);

            }
            result = target;
            return true;
        }

        public override bool Serialize(SerializedObject target, IObjectGraphNodeProvider provider, ObjectGraphView graphView, out SerializedObject result) {

            ObjectGraphNode[] nodes = provider.CollectNodes(graphView);

            ObjectGraphVariableNode[] variableNodes = graphView.Query<ObjectGraphVariableNode>(null).Where((variableNode) => variableNode.output.connected && variableNode.output.connections.Any((connection) => nodes.Contains(connection.input.node))).ToList().ToArray();
            var nodesProperty = target.FindProperty("nodes");
            var objectsProperty = target.FindProperty("objects");
            var variablesProperty = target.FindProperty("variables");
            nodesProperty.arraySize = nodes.Length;
            objectsProperty.arraySize = nodes.Length;
            variablesProperty.arraySize = variableNodes.Length;

            for (int i = 0; i < nodes.Length; i++) {
                var nodeProperty = nodesProperty.GetArrayElementAtIndex(i);
                nodeProperty.FindPropertyRelative("layout").rectValue = nodes[i].GetPosition();
                nodeProperty.FindPropertyRelative("id").stringValue = nodes[i].viewDataKey;
                var obj = Activator.CreateInstance(graphView.Model.entries[nodes[i].viewDataKey].type);
                foreach (var kv in graphView.Model.entries[nodes[i].viewDataKey].values) {
                    if (!graphView.Model.HasVariable(nodes[i].viewDataKey, kv.Key)) {
                        var fieldInfo = graphView.Model.entries[nodes[i].viewDataKey].type.GetField(kv.Key);
                        var fieldValue = kv.Value.value;
                        ObjectGraphNodeValueConverters.TryToConvertToOriginal(kv.Value.value, nodes, out fieldValue);
                        fieldInfo.SetValue(obj, fieldValue);
                    }
                    else {

                    }
                }
                nodeProperty.FindPropertyRelative("index").intValue = i;
                if (nodes[i].output.connected) {
                    var nodeGuid = nodes[i].output.connections.First().input.node.viewDataKey;
                    if (nodeGuid == graphView.MasterNode.viewDataKey) {
                        nodeProperty.FindPropertyRelative("next").intValue = -2;
                    }
                    else {
                        nodeProperty.FindPropertyRelative("next").intValue = Array.FindIndex(nodes, (n) => n.viewDataKey == nodeGuid);
                    }
                }
                else {
                    nodeProperty.FindPropertyRelative("next").intValue = -1;
                }
                objectsProperty.GetArrayElementAtIndex(i).managedReferenceValue = obj;
            }
            for (int i = 0; i < variableNodes.Length; i++) {
                var variableProperty = variablesProperty.GetArrayElementAtIndex(i);
                var variableEntry = graphView.Model.GetVariableEntry(variableNodes[i].viewDataKey);
                variableProperty.FindPropertyRelative("layout").rectValue = variableNodes[i].GetPosition();
                variableProperty.FindPropertyRelative("id").stringValue = variableNodes[i].viewDataKey;
                variableProperty.FindPropertyRelative("type").stringValue = variableEntry.variable.containerType.AssemblyQualifiedName;
                variableProperty.FindPropertyRelative("address").stringValue = variableEntry.variable.address;
                var variableConnectionsProperty = variableProperty.FindPropertyRelative("connections");
                variableConnectionsProperty.arraySize = variableNodes[i].output.connections.Count();
                for (int j = 0; j < variableEntry.targets.Count; j++) {
                    var variableConnectionProperty = variableConnectionsProperty.GetArrayElementAtIndex(j);
                    variableConnectionProperty.FindPropertyRelative("index").intValue = Array.FindIndex(nodes, (n) => n.viewDataKey == variableEntry.targets[j].id);
                    variableConnectionProperty.FindPropertyRelative("field").stringValue = variableEntry.targets[j].field;
                }
            }
            target.FindProperty("version").intValue += 1;
            target.ApplyModifiedProperties();
            result = target;
            return true;
        }

    }
}