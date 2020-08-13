using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reactics.Core.Editor {

    public static class TypeCommons {
        private static Dictionary<Type, Color32> typeColors = new Dictionary<Type, Color32>();
        public static Color GetColor(Type type) {
            if (!typeColors.TryGetValue(type, out Color32 color)) {
                int hash = 0;
                for (var i = 0; i < type.Name.Length; i++) {
                    hash = type.Name[i] + ((hash << 5) - hash);
                }
                var bytes = BitConverter.GetBytes(hash & 0x00FFFFFF);
                color = new Color32(bytes[0], bytes[1], bytes[2], 255);
            }
            return color;
        }
        private static Dictionary<Type, Boolean> unmanagedTypes = new Dictionary<Type, bool>();
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
    }
}