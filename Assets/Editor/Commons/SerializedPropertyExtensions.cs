using System;
using System.Collections.Generic;
using System.Linq;
using Reactics.Core.Commons;
using UnityEditor;
using UnityEngine;

namespace Reactics.Editor {
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
        /* public static void WriteFixedBuffer<TBuffer>(this SerializedProperty property) {
            //property.GetFixedBufferElementAtIndex().
        } */
    }
}