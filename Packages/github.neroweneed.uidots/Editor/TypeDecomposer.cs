using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace NeroWeNeed.UIDots {
    public class TypeDecomposer {
        //private Dictionary<Type, Dictionary<string, FieldData>> cachedTypes = new Dictionary<Type, Dictionary<string, FieldData>>();
        public Dictionary<string, FieldData> Decompose<TType>(int offset = 0, char separator = '.') => Decompose(typeof(TType), offset, separator);
        public Dictionary<string, FieldData> Decompose(Type type, int offset = 0, char separator = '.') => type == null ? null : GetDecomposedFields(type, offset, separator);
        public void Decompose<TType>(Dictionary<string, FieldData> fields, string prefix, int offset = 0, char separator = '.') => Decompose(typeof(TType), fields, prefix, separator);
        public void Decompose(Type type, Dictionary<string, FieldData> fields, string prefix, int offset = 0, char separator = '.') {
            if (type == null)
                return;
            var data = GetDecomposedFields(type, offset, separator);
            var p = string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix + separator;
            foreach (var kv in data) {
                fields[p + kv.Key] = kv.Value;
            }
        }
        private void Decompose(FieldInfo fieldInfo, string prefix, int baseOffset, int offset, Dictionary<string, FieldData> data, char separator = '.') {
            string path = prefix;
            if (fieldInfo.GetCustomAttribute<EmbedAttribute>() == null) {
                path = string.IsNullOrEmpty(path) ? fieldInfo.Name : $"{path}{separator}{fieldInfo.Name}";
                if (data.ContainsKey(path)) {
                    throw new Exception("Path already exists");
                }
                data[path] = new FieldData(baseOffset + offset, fieldInfo);
            }
            if (fieldInfo.FieldType != null && Type.GetTypeCode(fieldInfo.FieldType) == TypeCode.Object && fieldInfo.FieldType.GetCustomAttribute<TerminalAttribute>() == null && (!fieldInfo.FieldType.IsGenericType || !typeof(FunctionPointer<>).IsAssignableFrom(fieldInfo.FieldType.GetGenericTypeDefinition()))) {
                foreach (var innerFieldInfo in fieldInfo.FieldType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                    if (innerFieldInfo.GetCustomAttribute<HideInDecompositionAttribute>() == null)
                        Decompose(innerFieldInfo, path, baseOffset, UnsafeUtility.GetFieldOffset(fieldInfo), data, separator);
                }
            }
        }
        private Dictionary<string, FieldData> GetDecomposedFields(Type type, int offset, char separator) {
            var data = new Dictionary<string, FieldData>();
            foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (fieldInfo.GetCustomAttribute<HideInDecompositionAttribute>() == null)
                    Decompose(fieldInfo, null, offset, 0, data, separator);
            }
            //cachedTypes[type] = data;
            return data;
        }
        public List<string> DecomposePath<TType>(char separator = '.') => DecomposePath(typeof(TType), separator);
        public List<string> DecomposePath(Type type, char separator = '.') => type == null ? null : GetDecomposedPaths(type, separator);
        public void DecomposePath<TType>(List<string> paths, string prefix, char separator = '.') => DecomposePath(typeof(TType), paths, prefix, separator);
        public void DecomposePath(Type type, List<string> paths, string prefix, char separator = '.') {
            if (type == null)
                return;
            var data = GetDecomposedPaths(type, separator);
            var p = string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix + separator;
            foreach (var kv in data) {
                paths.Add(p + kv);
            }
        }
        private void DecomposePath(FieldInfo fieldInfo, string prefix, List<string> data, char separator = '.') {
            string path = prefix;
            if (fieldInfo.GetCustomAttribute<EmbedAttribute>() == null) {
                path = string.IsNullOrEmpty(path) ? fieldInfo.Name : $"{path}{separator}{fieldInfo.Name}";
                if (data.Contains(path)) {
                    throw new Exception("Path already exists");
                }
                data.Add(path);
            }
            if (fieldInfo.FieldType != null && Type.GetTypeCode(fieldInfo.FieldType) == TypeCode.Object && fieldInfo.FieldType.GetCustomAttribute<TerminalAttribute>() == null && (!fieldInfo.FieldType.IsGenericType || !typeof(FunctionPointer<>).IsAssignableFrom(fieldInfo.FieldType.GetGenericTypeDefinition()))) {
                foreach (var innerFieldInfo in fieldInfo.FieldType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                    if (innerFieldInfo.GetCustomAttribute<HideInDecompositionAttribute>() == null)
                        DecomposePath(innerFieldInfo, path, data, separator);
                }
            }
        }
        private List<string> GetDecomposedPaths(Type type, char separator) {
            var data = new List<string>();
            foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
                if (fieldInfo.GetCustomAttribute<HideInDecompositionAttribute>() == null)
                    DecomposePath(fieldInfo, null, data, separator);
            }
            //cachedTypes[type] = data;
            return data;
        }
        public struct FieldData {
            public int offset;
            public int length;
            public Type type;
            public bool isAssetReference;

            public FieldData(int offset, FieldInfo info) {
                this.offset = offset + UnsafeUtility.GetFieldOffset(info);
                length = UnsafeUtility.SizeOf(info.FieldType);
                type = info.FieldType;
                isAssetReference = info.GetCustomAttribute<AssetReferenceAttribute>() != null;
            }
        }

    }
}