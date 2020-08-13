using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Reactics.Core.Commons.Reflection {
    public static class SerializationUtility {
        public static bool IsSerializableField(this FieldInfo fieldInfo) {
            return (fieldInfo.IsPublic && fieldInfo.GetCustomAttribute<NonSerializedAttribute>() == null) || fieldInfo.GetCustomAttribute<SerializeField>() != null;
        }

        public static Type GetFieldType(this SerializedProperty property) {
            foreach (var field in property.serializedObject.targetObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (field.Name == property.name)
                    return field.FieldType;
            }
            return null;

        }
        public static FieldInfo GetField(this SerializedProperty property) {
            foreach (var field in property.serializedObject.targetObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (field.Name == property.name)
                    return field;
            }
            return null;

        }
    }
}