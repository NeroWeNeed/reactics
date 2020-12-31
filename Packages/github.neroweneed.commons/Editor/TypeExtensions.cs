using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    public static class TypeExtensions {
        private static readonly Dictionary<object, Color> hardCodedColors = new Dictionary<object, Color>();
        [InitializeOnLoadMethod]
        private static void InitPrimitiveColors() {
            hardCodedColors[typeof(long).AssemblyQualifiedName] = new Color(1f, 89 / 255f, 89 / 255f, 1f);
            hardCodedColors[typeof(int).AssemblyQualifiedName] = new Color(1f, 89 / 255f, 89 / 255f, 1f);
            hardCodedColors[typeof(short).AssemblyQualifiedName] = new Color(1f, 89 / 255f, 89 / 255f, 1f);
            hardCodedColors[typeof(sbyte).AssemblyQualifiedName] = new Color(1f, 89 / 255f, 89 / 255f, 1f);

            hardCodedColors[typeof(ulong).AssemblyQualifiedName] = new Color(1f, 1f, 89 / 255f, 1f);
            hardCodedColors[typeof(uint).AssemblyQualifiedName] = new Color(1f, 1f, 89 / 255f, 1f);
            hardCodedColors[typeof(ushort).AssemblyQualifiedName] = new Color(1f, 1f, 89 / 255f, 1f);
            hardCodedColors[typeof(byte).AssemblyQualifiedName] = new Color(1f, 1f, 89 / 255f, 1f);

            hardCodedColors[typeof(char).AssemblyQualifiedName] = new Color(89 / 255f, 1f, 89 / 255f, 1f);

            hardCodedColors[typeof(bool).AssemblyQualifiedName] = new Color(89 / 255f, 1f, 1f, 1f);

            hardCodedColors[typeof(float).AssemblyQualifiedName] = new Color(89 / 255f, 89 / 255f, 1f, 1f);
            hardCodedColors[typeof(double).AssemblyQualifiedName] = new Color(89 / 255f, 89 / 255f, 1f, 1f);
        }
        public static FieldInfo[] GetSerializableFields(this Type type) => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(t => (t.IsPublic && t.GetCustomAttribute<NonSerializedAttribute>() == null) || (!t.IsPublic && t.GetCustomAttribute<SerializeField>() != null)).ToArray();
        public static FieldInfo[] GetSerializableFields(this Type type, Predicate<Type> predicate) => type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where(t => ((t.IsPublic && t.GetCustomAttribute<NonSerializedAttribute>() == null) || (!t.IsPublic && t.GetCustomAttribute<SerializeField>() != null)) && predicate.Invoke(t.FieldType)).ToArray();

        public static Color GetColor(this Type type, bool ignoreAlpha = true) => GetColor(type.GetCustomAttribute<ColorAttribute>(), type.AssemblyQualifiedName, ignoreAlpha);
        public static Color GetColor(this FieldInfo fieldInfo, bool ignoreAlpha = true) => GetColor(fieldInfo.GetCustomAttribute<ColorAttribute>() ?? fieldInfo.FieldType.GetCustomAttribute<ColorAttribute>(), fieldInfo.FieldType.AssemblyQualifiedName, ignoreAlpha);
        public static Color GetColor(this PropertyInfo propertyInfo, bool ignoreAlpha = true) => GetColor(propertyInfo.GetCustomAttribute<ColorAttribute>() ?? propertyInfo.PropertyType.GetCustomAttribute<ColorAttribute>(), propertyInfo.PropertyType.AssemblyQualifiedName, ignoreAlpha);
        public static Color GetColor(this MethodInfo methodInfo, bool ignoreAlpha = true) => GetColor(methodInfo.GetCustomAttribute<ColorAttribute>() ?? methodInfo.ReturnType.GetCustomAttribute<ColorAttribute>(), methodInfo.ReturnType.AssemblyQualifiedName, ignoreAlpha);
        private static Color GetColor(ColorAttribute attribute, object source, bool ignoreAlpha) {
            if (attribute != null) {
                Color color = attribute.Value;
                if (ignoreAlpha) {
                    color.a = 1f;
                }
                return color;
            }
            else if (hardCodedColors.TryGetValue(source, out Color color)) {
                if (ignoreAlpha) {
                    color.a = 1f;
                }
                return color;
            }
            else {
                var hash = source.GetHashCode();
                var a = ignoreAlpha ? 1f : (0b00000000000000000000000011111111 & hash) / 255f;
                var b = ((0b00000000000000001111111100000000 & hash) >> 8) / 255f;
                var g = ((0b00000000111111110000000000000000 & hash) >> 16) / 255f;
                var r = ((0b11111111000000000000000000000000 & hash) >> 24) / 255f;
                return new Color(r, g, b, a);
            }
        }

        public static bool IsSerializable(this FieldInfo field) => (field.IsPublic && field.GetCustomAttribute<NonSerializedAttribute>() == null) || (!field.IsPublic && field.GetCustomAttribute<SerializeField>() != null);
        public static object GetDefault(this Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        public static FieldOffsetInfo[] Decompose(this Type type, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) {
            var info = new List<FieldOffsetInfo>();
            Decompose(type, 0, null, info, flags);
            info.Sort();
            return info.ToArray();
        }
        private static void Decompose(Type type, int currentOffset, FieldInfo parent, List<FieldOffsetInfo> fieldOffsetInfos, BindingFlags flags) {
            foreach (var fieldInfo in type.GetFields(flags)) {
                var offsetInfo = new FieldOffsetInfo(currentOffset, parent, fieldInfo);
                fieldOffsetInfos.Add(offsetInfo);
                if (Type.GetTypeCode(fieldInfo.FieldType) == TypeCode.Object) {
                    Decompose(fieldInfo.FieldType, offsetInfo.offset, fieldInfo, fieldOffsetInfos, flags);
                }
            }
        }
        public static Type[] GetLoadableTypes(this Assembly assembly) {
            try {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e) {
                return e.Types.Where(t => t != null).ToArray();
            }
        }
    }
    [Serializable]
    public struct FieldOffsetInfo : IComparable<FieldOffsetInfo> {
        public bool root;
        public string fullName;
        [HideInInspector]
        public SerializableType type;
        public string name;
        public int offset;
        public int length;
        public FieldOffsetInfo(int currentOffset, FieldInfo parent, FieldInfo fieldInfo) {
            this.root = parent == null;
            fullName = root ? fieldInfo.Name : $"{parent.Name}.{fieldInfo.Name}";
            name = fieldInfo.Name;
            this.type = fieldInfo.FieldType;
            this.offset = currentOffset + UnsafeUtility.GetFieldOffset(fieldInfo);
            length = UnsafeUtility.SizeOf(fieldInfo.FieldType);
        }
        public int CompareTo(FieldOffsetInfo other) {
            var c1 = offset.CompareTo(other.offset);
            return c1 != 0 ? c1 : other.length.CompareTo(length);
        }
    }

}