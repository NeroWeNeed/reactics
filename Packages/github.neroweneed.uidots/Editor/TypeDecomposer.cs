using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public class TypeDecomposer {
        private Dictionary<Type, Dictionary<string, FieldData>> cachedTypes = new Dictionary<Type, Dictionary<string, FieldData>>();
        public Dictionary<string, FieldData> Decompose<TType>(char separator = '.') => Decompose(typeof(TType), separator);
        public Dictionary<string, FieldData> Decompose(Type type, char separator = '.') {
            if (type == null)
                return null;

            if (!cachedTypes.TryGetValue(type, out Dictionary<string, FieldData> data)) {
                data = new Dictionary<string, FieldData>();
                foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                    if (fieldInfo.GetCustomAttribute<HideInDecompositionAttribute>() == null)
                        Decompose(fieldInfo, null, 0, data, separator);
                }
                cachedTypes[type] = data;
            }
            return data;
        }
        private void Decompose(FieldInfo fieldInfo, string prefix, int offset, Dictionary<string, FieldData> data, char separator = '.') {
            string path = prefix;
            if (fieldInfo.GetCustomAttribute<EmbedAttribute>() == null) {
                path = string.IsNullOrEmpty(path) ? fieldInfo.Name : $"{path}{separator}{fieldInfo.Name}";
                if (data.ContainsKey(path)) {
                    throw new Exception("Path already exists");
                }
                data[path] = new FieldData(offset, fieldInfo);
            }
            if (fieldInfo.FieldType != null && Type.GetTypeCode(fieldInfo.FieldType) == TypeCode.Object && fieldInfo.FieldType.GetCustomAttribute<TerminalAttribute>() == null) {
                foreach (var innerFieldInfo in fieldInfo.FieldType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                    if (innerFieldInfo.GetCustomAttribute<HideInDecompositionAttribute>() == null)
                        Decompose(innerFieldInfo, path, UnsafeUtility.GetFieldOffset(fieldInfo), data, separator);
                }
            }
        }
        public struct FieldData {
            public int offset;
            public int length;
            public Type type;
            public bool isAssetReference;
            public FieldData(FieldData old, int adjust) {
                this.offset = old.offset + adjust;
                this.length = old.length;
                this.type = old.type;
                this.isAssetReference = old.isAssetReference;
            }
            public FieldData(int offset, FieldInfo info) {
                this.offset = offset + UnsafeUtility.GetFieldOffset(info);
                length = UnsafeUtility.SizeOf(info.FieldType);
                type = info.FieldType;
                isAssetReference = info.GetCustomAttribute<AssetReferenceAttribute>() != null;
            }
        }
    }
}