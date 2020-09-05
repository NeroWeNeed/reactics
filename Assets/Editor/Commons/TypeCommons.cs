using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Reactics.Core.Commons;
using UnityEngine;

namespace Reactics.Editor {

    public static class TypeCommons {
        private static readonly Dictionary<Type, Color32> typeColors = new Dictionary<Type, Color32>();
        public static Color GetColor(this Type type) => GetColor(type, Color.black);
        public static Color GetColor(this Type type, Color defaultColor) {
            if (type == null)
                return defaultColor;
            if (type.IsConstructedGenericType) {
                var constructedColorAttr = type.GetCustomAttributes<ConcreteTypeColorAttribute>()?.FirstOrDefault((attr) => attr.GenericDefinition == type.GenericTypeArguments);
                if (constructedColorAttr != null)
                    return constructedColorAttr.Color;
            }
            var colorAttr = type.GetCustomAttribute<TypeColorAttribute>();
            if (colorAttr != null) {
                return colorAttr.Color;
            }
            if (!typeColors.TryGetValue(type, out Color32 color)) {
                int hash = 0;
                for (var i = 0; i < type.FullName.Length; i++) {
                    hash = type.FullName[i] + ((hash << 5) - hash);
                }
                var bytes = BitConverter.GetBytes(hash & 0x00FFFFFF);
                color = new Color32(bytes[0], bytes[1], bytes[2], 255);
            }
            return color;
        }
        private static readonly Dictionary<Type, bool> unmanagedTypes = new Dictionary<Type, bool>();
        public static bool IsUnmanaged(this Type type) {
            if (type.IsPrimitive || type.IsEnum || type.IsPointer) {
                return true;
            }
            else if (unmanagedTypes.TryGetValue(type, out bool value)) {
                return value;
            }
            else if (type.IsValueType) {
                foreach (var field in type.GetFields()) {
                    if (field.FieldType.Equals(type))
                        continue;
                    if (!IsUnmanaged(field.FieldType)) {
                        unmanagedTypes.Add(type, false);
                        return false;
                    }
                }
                unmanagedTypes.Add(type, true);
                return true;
            }
            else {

                return false;
            }

        }

        public static string GetRealName(this Type t) {
            if (!t.IsGenericType)
                return t.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append(t.Name, 0, t.Name.IndexOf('`'));
            sb.Append('<');
            bool appendComma = false;
            foreach (Type arg in t.GetGenericArguments()) {
                if (appendComma) sb.Append(',');
                sb.Append(GetRealName(arg));
                appendComma = true;
            }
            sb.Append('>');
            return sb.ToString();
        }
        public static string GetRealFullName(this Type t) {
            if (!t.IsGenericType)
                return t.Name;

            StringBuilder sb = new StringBuilder();
            sb.Append(t.Name, 0, t.FullName.IndexOf('`'));
            sb.Append('<');
            bool appendComma = false;
            foreach (Type arg in t.GetGenericArguments()) {
                if (appendComma) sb.Append(',');
                sb.Append(GetRealFullName(arg));
                appendComma = true;
            }
            sb.Append('>');
            return sb.ToString();
        }
    }
}