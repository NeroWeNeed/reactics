using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Reactics.Core.Commons {
    public static class GeneralCommons {
        public static Rect Offset(this Rect rect, Vector2 value) {
            rect.xMin += value.x;
            rect.yMin += value.y;
            rect.xMax += value.x;
            rect.yMax += value.y;
            return rect;
        }
        public static Color ParseColor(string hex, float alpha = 1) {
            var hexValue = hex.StartsWith("#") ? hex.Substring(1) : hex;
            switch (hexValue.Length) {
                case 6:
                    return new Color(int.Parse(hexValue.Substring(0, 2), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(2, 2), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(4, 2), NumberStyles.HexNumber) / 255f, alpha);

                case 3:
                    return new Color(int.Parse(hexValue.Substring(0, 1), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(1, 1), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(2, 1), NumberStyles.HexNumber) / 255f, alpha);

                case 8:
                    return new Color(int.Parse(hexValue.Substring(0, 2), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(2, 2), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(4, 2)) / 255f, int.Parse(hexValue.Substring(6, 2), NumberStyles.HexNumber) / 255f);

                case 4:
                    return new Color(int.Parse(hexValue.Substring(0, 1), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(1, 1), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(2, 1), NumberStyles.HexNumber) / 255f, int.Parse(hexValue.Substring(3, 1), NumberStyles.HexNumber) / 255f);
                default:
                    throw new UnityException("Invalid Hex String");
            }


        }
        public static V TryGetValue<K, V>(this Dictionary<K, V> self, K key, V defaultValue) => self.TryGetValue(key, out V value) ? value : defaultValue;
        public static void Fill<T>(this T[] array, T value) {
            for (int i = 0; i < array.Length; i++)
                array[i] = value;
        }
        public static void FillNull<T>(this T[] array, T value) where T : class {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == null)
                    array[i] = value;
        }
        public static void FillDefault<T>(this T[] array, T value) where T : struct {
            for (int i = 0; i < array.Length; i++)
                if (array[i].Equals(default))
                    array[i] = value;
        }
        public static void Difference<T>(this IEnumerable<T> self, IEnumerable<T> other, out T[] added, out T[] removed) {
            var addedList = new List<T>();
            var removedList = new List<T>();
            foreach (var item in self) {
                if (!other.Contains(item))
                    removedList.Add(item);
            }
            foreach (var item in other) {
                if (!self.Contains(item))
                    addedList.Add(item);
            }
            added = addedList.ToArray();
            removed = removedList.ToArray();


        }
        public static bool IsDefined(this float self) => !float.IsInfinity(self) && !float.IsNaN(self);

        public static void DefinedOrZero(ref this float2 self) {
            if (!self.x.IsDefined())
                self.x = 0;
            if (!self.y.IsDefined())
                self.y = 0;
        }

        public static float4 ToFloat4(this Color color) => new float4(color.r, color.g, color.b, color.a);

    }
    [Serializable]
    public class AssetReference<TAsset> : AssetReferenceT<TAsset> where TAsset : UnityEngine.Object {
        public AssetReference(string guid) : base(guid) {
        }
    }

}