using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NeroWeNeed.UIDots.Editor {
    public interface IPropertyWriter {
        bool CanParse(string s);
        void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset);
    }
    public interface IPropertyWriter<TValue> : IPropertyWriter where TValue : struct {

    }
    public class PropertyWriter<TValue> : IPropertyWriter<TValue> where TValue : struct {
        private readonly UIPropertyParser<TValue> parser;

        public PropertyWriter(UIPropertyParser<TValue> parser) {
            this.parser = parser;
        }

        public bool CanParse(string s) {
            return parser.Invoke(s, out TValue _);
        }

        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {

            if (parser.Invoke(s, out TValue result)) {
                UnsafeUtility.CopyStructureToPtr(ref result, (ptr + fieldData.offset).ToPointer());
            }

        }

    }
    public delegate bool UIPropertyParser<TValue>(string s, out TValue result);
    public class UVPropertyWriter : IPropertyWriter<UVData> {

        public bool CanParse(string s) {
            var asset = AssetDatabase.LoadAssetAtPath<Sprite>(s.StartsWith("guid:") ? AssetDatabase.GUIDToAssetPath(s.Substring(5)) : s);
            return asset != null;
        }

        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(s.StartsWith("guid:") ? AssetDatabase.GUIDToAssetPath(s.Substring(5)) : s);
            if (sprite != null) {
                var uvs = SpriteUtility.GetSpriteUVs(sprite, true);
                var uvData = new UVData
                {
                    value = new float4(uvs[0].x, uvs[0].y, math.abs(uvs[1].x - uvs[0].x), math.abs(uvs[1].y - uvs[0].y))
                };
                UnsafeUtility.CopyStructureToPtr(ref uvData, (ptr + fieldData.offset).ToPointer());
            }
        }
    }
    public class LocalizedStringPointerPropertyWriter : IPropertyWriter<LocalizedStringPtr> {

        public bool CanParse(string s) {
            return true;
        }

        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {
            var current = extraBytesStream.Length;
            var bytes = Encoding.Unicode.GetBytes(s);
            extraBytesStream.Write(bytes);
            var str = new LocalizedStringPtr
            {
                offset = current + extraByteStreamOffset,
                length = (extraBytesStream.Length - current) / 2
            };
            UnsafeUtility.CopyStructureToPtr(ref str, (ptr + fieldData.offset).ToPointer());
        }
    }
    public class LocalizedAssetPointerPropertyWriter : IPropertyWriter<LocalizedAssetPtr> {

        public bool CanParse(string s) {
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(s.StartsWith("guid:") ? AssetDatabase.AssetPathToGUID(s.Substring(5)) : s) != null;
        }
        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {
            if (CanParse(s)) {
                var guid = s.StartsWith("guid:") ? s.Substring(5) : AssetDatabase.AssetPathToGUID(s);
                if (GUID.TryParse(guid, out GUID result)) {

                    UnsafeUtility.CopyStructureToPtr(ref result, (ptr + fieldData.offset).ToPointer());
                }


            }
        }
    }
    public static class PropertyWriterFactory {
        public static IPropertyWriter Create<TType>() where TType : struct {
            var type = typeof(TType);
            UIPropertyParser<TType> parser = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == "TryParse" && x.GetParameters().Length == 2)?.CreateDelegate(typeof(UIPropertyParser<>).MakeGenericType(type)) as UIPropertyParser<TType>;
            //object parser = targetType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public)
            Contract.Ensures(parser != null);
            return new PropertyWriter<TType>(parser);
        }
        public static IPropertyWriter Create(Type type) {
            Contract.Ensures(type.IsValueType);
            object parser = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == "TryParse" && x.GetParameters().Length == 2)?.CreateDelegate(typeof(UIPropertyParser<>).MakeGenericType(type));
            //object parser = targetType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public)
            Contract.Ensures(parser != null);
            return Activator.CreateInstance(typeof(PropertyWriter<>).MakeGenericType(type), parser) as IPropertyWriter;
        }
        public static Dictionary<Type, IPropertyWriter> CreateDictonary(params Type[] types) {
            return types.ToDictionary(keySelector: type => type, elementSelector: type => Create(type));

        }
        public static void Write(this Dictionary<Type, IPropertyWriter> writers, string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {
            if (typeof(ICompositeData<>).IsAssignableFrom(fieldData.type)) {
                var target = fieldData.type.GenericTypeArguments[0];
                if (UnsafeUtility.SizeOf(target) * 4 == UnsafeUtility.SizeOf(fieldData.type))
                    writers.FirstOrDefault(x => x.Key.IsAssignableFrom(target)).Value?.WriteComposite(target, s, ptr, fieldData, extraBytesStream, extraByteStreamOffset);

            }
            else if (fieldData.type.IsEnum) {
                WriteEnum(fieldData.type, s, ptr, fieldData, extraBytesStream, extraByteStreamOffset);
            }
            else {
                writers.FirstOrDefault(x => x.Key.IsAssignableFrom(fieldData.type)).Value?.Write(s, ptr, fieldData, extraBytesStream, extraByteStreamOffset);
            }

        }
        private static void WriteEnum(Type type, string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {
            var parser = typeof(Enum).GetMethod(nameof(Enum.TryParse), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type).CreateDelegate(typeof(UIPropertyParser<>).MakeGenericType(type));
            var writer = Activator.CreateInstance(typeof(PropertyWriter<>).MakeGenericType(type), parser) as IPropertyWriter;
            writer.Write(s, ptr, fieldData, extraBytesStream, extraByteStreamOffset);
        }
        private static void DoCompositeWrite(this IPropertyWriter targetWriter, Type type, IntPtr ptr, TypeDecomposer.FieldData fieldData, string sx, string sy, string sz, string sw, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {
            var size = UnsafeUtility.SizeOf(type);
            targetWriter.Write(sx, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset,
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset);
            targetWriter.Write(sy, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset + size,
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset);
            targetWriter.Write(sz, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset + (size * 2),
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset);
            targetWriter.Write(sw, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset + (size * 3),
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset);
        }
        private static unsafe void WriteComposite(this IPropertyWriter targetWriter, Type type, string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset) {
            var elements = s.Split(' ').Where(x => targetWriter.CanParse(x)).Take(4).ToArray();
            switch (elements.Length) {
                case 0:
                    return;
                case 1:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[0], elements[0], elements[0], extraBytesStream, extraByteStreamOffset);
                    return;
                case 2:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[1], elements[0], elements[1], extraBytesStream, extraByteStreamOffset);
                    return;
                case 3:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[1], elements[1], elements[2], extraBytesStream, extraByteStreamOffset);
                    return;
                default:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[1], elements[2], elements[3], extraBytesStream, extraByteStreamOffset);
                    return;
            }
        }
    }
}