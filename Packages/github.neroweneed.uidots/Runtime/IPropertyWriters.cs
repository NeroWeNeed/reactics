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
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NeroWeNeed.UIDots {
    public interface IUIPropertyWriter {
        bool CanParse(string s);

        void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context);
    }
    public struct UIPropertyWriterContext {
        public UISpriteGroup spriteGroup;
        public string[] fonts;
    }
    public interface IUIPropertyWriter<TValue> : IUIPropertyWriter where TValue : struct {

    }
    public class UIPropertyWriter<TValue> : IUIPropertyWriter<TValue> where TValue : struct {
        private readonly UIPropertyParser<TValue> parser;

        public UIPropertyWriter(UIPropertyParser<TValue> parser) {
            this.parser = parser;
        }

        public bool CanParse(string s) {
            return parser.Invoke(s, out TValue _);
        }

        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {

            if (parser.Invoke(s, out TValue result)) {
                UnsafeUtility.CopyStructureToPtr(ref result, (ptr + fieldData.offset).ToPointer());
            }

        }

    }
    public delegate bool UIPropertyParser<TValue>(string s, out TValue result);
    public class UIUVPropertyWriter : IUIPropertyWriter<UVData> {

        public bool CanParse(string s) {
            return AssetDatabase.GUIDToAssetPath(s) != null;
        }

        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            if (context.spriteGroup != null && context.spriteGroup.TryGetUVs(s, out Rect uvs)) {
                var uvData = new UVData
                {
                    value = new float4(uvs.x, uvs.y, uvs.width, uvs.height)
                };
                UnsafeUtility.CopyStructureToPtr(ref uvData, (ptr + fieldData.offset).ToPointer());
            }
        }
    }
    public class UILocalizedStringPointerPropertyWriter : IUIPropertyWriter<LocalizedStringPtr> {

        public bool CanParse(string s) {
            return true;
        }

        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
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
    public class UIAssetPointerPropertyWriter : IUIPropertyWriter<BlittableAssetReference> {

        public bool CanParse(string s) {
            return AssetDatabase.GUIDToAssetPath(s) != null;
        }
        public unsafe void Write(string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            if (CanParse(s)) {
                if (GUID.TryParse(s, out GUID result)) {
                    UnsafeUtility.CopyStructureToPtr(ref result, (ptr + fieldData.offset).ToPointer());
                }


            }
        }
    }
    public static class UIPropertyWriterFactory {
        public static IUIPropertyWriter Create<TType>() where TType : struct {
            var type = typeof(TType);
            UIPropertyParser<TType> parser = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == "TryParse" && x.GetParameters().Length == 2)?.CreateDelegate(typeof(UIPropertyParser<>).MakeGenericType(type)) as UIPropertyParser<TType>;
            //object parser = targetType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public)
            Contract.Ensures(parser != null);
            return new UIPropertyWriter<TType>(parser);
        }
        public static IUIPropertyWriter Create(Type type) {
            Contract.Ensures(type.IsValueType);
            object parser = type.GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(x => x.Name == "TryParse" && x.GetParameters().Length == 2)?.CreateDelegate(typeof(UIPropertyParser<>).MakeGenericType(type));
            //object parser = targetType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public)
            Contract.Ensures(parser != null);
            return Activator.CreateInstance(typeof(UIPropertyWriter<>).MakeGenericType(type), parser) as IUIPropertyWriter;
        }
        public static Dictionary<Type, IUIPropertyWriter> CreateDictonary(params Type[] types) {
            return types.ToDictionary(keySelector: type => type, elementSelector: type => Create(type));

        }
        public static void Write(this Dictionary<Type, IUIPropertyWriter> writers, string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            if (typeof(ICompositeData).IsAssignableFrom(fieldData.type)) {
                var target = fieldData.type.GenericTypeArguments[0];
                if (UnsafeUtility.SizeOf(target) * 4 == UnsafeUtility.SizeOf(fieldData.type))
                    writers.FirstOrDefault(x => x.Key.IsAssignableFrom(target)).Value?.WriteComposite(target, s, ptr, fieldData, extraBytesStream, extraByteStreamOffset, context);

            }
            else if (fieldData.type.IsEnum) {
                WriteEnum(fieldData.type, s, ptr, fieldData, extraBytesStream, extraByteStreamOffset, context);
            }
            else {
                writers.FirstOrDefault(x => x.Key.IsAssignableFrom(fieldData.type)).Value?.Write(s, ptr, fieldData, extraBytesStream, extraByteStreamOffset, context);
            }

        }
        private static void WriteEnum(Type type, string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            var parser = typeof(Enum).GetMethod(nameof(Enum.TryParse), BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(type).CreateDelegate(typeof(UIPropertyParser<>).MakeGenericType(type));
            var writer = Activator.CreateInstance(typeof(UIPropertyWriter<>).MakeGenericType(type), parser) as IUIPropertyWriter;
            writer.Write(s, ptr, fieldData, extraBytesStream, extraByteStreamOffset, context);
        }
        private static void DoCompositeWrite(this IUIPropertyWriter targetWriter, Type type, IntPtr ptr, TypeDecomposer.FieldData fieldData, string sx, string sy, string sz, string sw, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            var size = UnsafeUtility.SizeOf(type);
            targetWriter.Write(sx, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset,
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset, context);
            targetWriter.Write(sy, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset + size,
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset, context);
            targetWriter.Write(sz, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset + (size * 2),
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset, context);
            targetWriter.Write(sw, ptr, new TypeDecomposer.FieldData
            {
                offset = fieldData.offset + (size * 3),
                length = size,
                type = type,
                isAssetReference = false
            }, extraBytesStream, extraByteStreamOffset, context);
        }
        private static unsafe void WriteComposite(this IUIPropertyWriter targetWriter, Type type, string s, IntPtr ptr, TypeDecomposer.FieldData fieldData, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            var elements = s.Split(' ').Where(x => targetWriter.CanParse(x)).Take(4).ToArray();
            switch (elements.Length) {
                case 0:
                    return;
                case 1:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[0], elements[0], elements[0], extraBytesStream, extraByteStreamOffset, context);
                    return;
                case 2:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[1], elements[0], elements[1], extraBytesStream, extraByteStreamOffset, context);
                    return;
                case 3:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[1], elements[1], elements[2], extraBytesStream, extraByteStreamOffset, context);
                    return;
                default:
                    targetWriter.DoCompositeWrite(type, ptr, fieldData, elements[0], elements[1], elements[2], elements[3], extraBytesStream, extraByteStreamOffset, context);
                    return;
            }
        }
    }
}