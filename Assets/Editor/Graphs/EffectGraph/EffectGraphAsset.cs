using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Reactics.Core.Effects;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Reactics.Editor.Graph {
    [CreateAssetMenu(fileName = "EffectGraphAsset", menuName = "reactics/EffectGraphAsset", order = 0)]
    public class EffectGraphAsset : ObjectGraphAsset<IEffect, EffectAsset> {
        public override Type GetOutputAssetType() {
            var rootType = GetRootType();
            if (rootType == null)
                return null;

            return typeof(EffectAsset<>).MakeGenericType(rootType);
        }


        public override string DefaultOutputPath { get; } = "Assets/ResourceData/Effects";
        public Type GetRootType() {
            var nexts = this.nodes.Select((node) => node.next);
            var roots = this.nodes.Where((_, index) => !nexts.Contains(index)).ToArray();
            var type = roots.Select((root) => this.objects[root.index].GetType()).Distinct().ToArray();
            if (type.Length == 0) {
                Debug.LogError($"Unable to Find Root Type in {name}.");
                return null;
            }
            if (type.Length > 1) {
                Debug.LogError($"Multiple Root Types found in {name}.");
                return null;
            }
            return type[0];
        }
        public override void UpdateAsset(SerializedObject serializedObject) {
            var rootsProperty = serializedObject.FindProperty("roots");
            var variablesProperty = serializedObject.FindProperty("variables");
            var operationsProperty = serializedObject.FindProperty("operations");
            var componentsProperty = serializedObject.FindProperty("components");
            //var targetTypeProperty = serializedObject.FindProperty("targetType");
            var nexts = this.nodes.Select((node) => node.next);
            var roots = this.nodes.Where((_, index) => !nexts.Contains(index)).ToArray();
            var type = GetRootType();


            rootsProperty.arraySize = roots.Length;
            var components = new List<IEffect>(objects);
            for (int i = 0; i < nodes.Length; i++) {
                if (nodes[i].next >= 0) {
                    var old = components[nodes[i].index];
                    components[nodes[i].index] = new LinearEffect
                    {
                        next = nodes[i].next,
                        effect = components.Count
                    };
                    components.Add(old);
                }
            }
            var operations = new List<VariableOperationSequence>();
            var concreteVariables = new List<Variable>();
            for (int i = 0; i < variables.Length; i++) {
                var variable = variables[i];
                for (int j = 0; j < variable.connections.Length; j++) {
                    var connection = variable.connections[j];
                    var operationSequence = new VariableOperationSequence();
                    var variableReference = variable.CreateVariable();
                    if (operationSequence.variable < 0) {
                        operationSequence.variable = concreteVariables.Count;
                        concreteVariables.Add(variableReference);
                    }
                    var targetIndex = connection.index;
                    var targetComponent = components[targetIndex];

                    if (targetComponent is LinearEffect linearEffect) {
                        targetIndex = linearEffect.effect;
                        targetComponent = components[targetIndex];
                    }
                    var field = targetComponent.GetType().GetField(connection.field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field == null) {
                        throw new System.InvalidOperationException($"Invalid Field {field} for type {targetComponent.GetType()}");
                    }
                    operationSequence.offset = Marshal.OffsetOf(targetComponent.GetType(), field.Name).ToInt32();
                    operationSequence.variable = concreteVariables.IndexOf(variableReference);
                    operationSequence.component = targetIndex;
                    operationSequence.length = UnsafeUtility.SizeOf(field.FieldType);
                    operationSequence.type = Type.GetTypeCode(field.FieldType);
                    operations.Add(operationSequence);
                }
            }
            var typeCodes = Enum.GetValues(typeof(TypeCode));
            operationsProperty.WriteArray(operations, (_, element, property) =>
            {
                property.FindPropertyRelative("variable").intValue = element.variable;
                property.FindPropertyRelative("component").intValue = element.component;
                property.FindPropertyRelative("offset").intValue = element.offset;
                property.FindPropertyRelative("length").longValue = element.length;
                property.FindPropertyRelative("type").enumValueIndex = Array.IndexOf(typeCodes, element.type);
            });
            variablesProperty.WriteArray(concreteVariables, (_, concreteVariable, property) =>
            {
                property.FindPropertyRelative("offset").intValue = concreteVariable.offset;
                property.FindPropertyRelative("length").longValue = concreteVariable.length;
                property.FindPropertyRelative("containerId").FindPropertyRelative("value").WriteGuid(concreteVariable.containerId);
            });
            rootsProperty.WriteArray(roots, (_, node, property) => property.intValue = node.index);
            componentsProperty.WriteArray(components, (_, element, property) => property.managedReferenceValue = element);

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(serializedObject.targetObject));
            var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Effects");
            var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid, group, true, true);
            entry.address = serializedObject.targetObject.name;
        }

    }
    [CustomEditor(typeof(EffectGraphAsset))]
    public class EffectGraphAssetInspector : ObjectGraphAssetEditor<EffectGraphEditor, EffectGraphAsset> {
        public override string GetEditorTitle(EffectGraphAsset asset) => EffectGraphEditor.CreateTitle(asset);
    }
}