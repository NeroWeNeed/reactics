using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NeroWeNeed.Commons;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    public static class SerializedPropertyExtensions {
        public static void WriteArray<TElement>(this SerializedProperty property, IEnumerable<TElement> array, Action<int, TElement, SerializedProperty> writer) {

            property.arraySize = array.Count();
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            property.serializedObject.UpdateIfRequiredOrScript();
            int index = 0;
            foreach (var item in array) {
                writer(index, item, property.GetArrayElementAtIndex(index));
                index++;
            }

        }
        public static void WriteByteBuffer(this SerializedProperty property, byte[] buffer) {
            for (int i = 0; i < buffer.Length; i++) {
                property.GetFixedBufferElementAtIndex(i).intValue = buffer[i];

            }
        }
        public static void WriteGuid(this SerializedProperty property, BlittableGuid guid) {
            var bytes = guid.ToByteArray();

            var bufferProp = property.GetFixedBufferElementAtIndex(0);
            /*             Debug.Log(property.propertyPath);
                        property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        property.serializedObject.UpdateIfRequiredOrScript(); */
            for (int i = 0; i < bytes.Length; i++) {
                bufferProp.intValue = bytes[i];
                bufferProp.Next(true);

            }
        }

        public static TResult[] ToArray<TResult>(this SerializedProperty property, Func<SerializedProperty, TResult> collector) {
            if (!property.isArray)
                return Array.Empty<TResult>();
            var result = new TResult[property.arraySize];
            for (int i = 0; i < property.arraySize; i++) {
                result[i] = collector.Invoke(property.GetArrayElementAtIndex(i));
            }
            return result;
        }
        public static FieldInfo GetFieldInfo(this SerializedProperty property) {
            var obj = property.serializedObject.targetObject;

            if (obj == null)
                return null;
            var pathNodes = property.propertyPath.Split('.');
            if (pathNodes.Length <= 0)
                return null;
            FieldInfo currentFieldInfo = obj.GetType().GetField(pathNodes[0], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            int i = 1;
            while (i < pathNodes.Length) {
                if (pathNodes[i] == "Array" && i + 1 < pathNodes.Length && pathNodes[i + 1].StartsWith("data[")) {
                    if (i + 2 < pathNodes.Length) {
                        currentFieldInfo = currentFieldInfo.FieldType.GetElementType().GetField(pathNodes[i + 2], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    }
                    i += 2;
                }
                else {
                    currentFieldInfo = currentFieldInfo.FieldType.GetField(pathNodes[i++], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                }
            }
            return currentFieldInfo;
        }
        /* public static void WriteFixedBuffer<TBuffer>(this SerializedProperty property) {
            //property.GetFixedBufferElementAtIndex().
        } */
    }
}