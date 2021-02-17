using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public unsafe static class UIModelWriter {

        //TODO: Global Settings
        public const string UI_ADDRESSABLES_GROUP = "User Interfaces";
        public const string UI_OUTPUT_PATH = "Assets/ResourceData/UI";
        public static Material GetMaterial(this UIModel model) => model.group?.Material;
        public static string GetOutputGuid(this UIModel model) => AssetDatabase.GUIDFromAssetPath(model.output).ToString();

        public static string Write(this UIModel model) {
            var fi = new FileInfo(model.output);
            fi.Directory.Create();
            using (var fs = fi.Create()) {
                Write(model, fs, UIGlobalSettings.GetOrCreateSettings().schema);
            }



            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(model.output);
            AssetDatabase.Refresh();
            //return AssetDatabase.GUIDFromAssetPath(model.output).ToString();
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            var guid = AssetDatabase.GUIDFromAssetPath(model.output).ToString();
            if (settings != null && !string.IsNullOrEmpty(model.address)) {
                var entry = settings.FindAssetEntry(guid);
                if (entry == null) {
                    entry = settings.CreateOrMoveEntry(guid, settings.FindGroup(UI_ADDRESSABLES_GROUP) ?? AddressableAssetSettingsDefaultObject.Settings.CreateGroup(UI_ADDRESSABLES_GROUP, false, false, false, null));
                }
                entry.SetAddress(model.address);
            }
            return guid;

        }
        /// <summary>
        /// Serializes a UIModel in the following format:
        /// Total Length: ulong (excluding this field)
        /// Node Count: int
        /// For each node present:
        /// Node Size: int (excluding this field)
        /// Header Data: HeaderConfig
        /// Children: int[], total length of children array can be found inside the HeaderConfig.
        /// ConfigBlocks: Which config blocks are present can be found by querying the configuration mask in the headerconfig
        /// </summary>
        public static void Write(this UIModel model, Stream stream, UISchema schema) {
            Contract.Assert(model.group != null);
            using var modelWriter = new MemoryBinaryWriter();
            modelWriter.Write(0UL);
            modelWriter.Write(model.nodes.Count);
            var context = new UIPropertyWriterContext
            {
                group = model.group
            };
            var decomposer = new TypeDecomposer();
            var types = new List<Type>();
            long totalSize = UnsafeUtility.SizeOf<int>();
            Configure(model, -1, 0, schema, modelWriter, decomposer, context, types, ref totalSize);
            /* for (int i = 0; i < model.nodes.Count; i++) {
                totalSize += Configure(model.nodes[i], modelWriter, decomposer, context, types) + UnsafeUtility.SizeOf<int>();
            } */
            UnsafeUtility.MemCpy(modelWriter.Data, UnsafeUtility.AddressOf(ref totalSize), UnsafeUtility.SizeOf<ulong>());
            using var modelStream = new UnmanagedMemoryStream(modelWriter.Data, modelWriter.Length);
            modelStream.CopyTo(stream);
        }
        private static void Configure(UIModel model, int parent, int index, UISchema schema, MemoryBinaryWriter modelWriter, TypeDecomposer decomposer, UIPropertyWriterContext context, List<Type> types, ref long totalSize) {
            var node = model.nodes[index];
            var header = new HeaderConfig
            {
                configurationMask = node.mask,
                schemaIndex = schema.elements.FindIndex((element) => element.identifier == node.identifier),
                flags = 0,
                childCount = node.children.Count,
                parent = parent
            };
            var sizeOffset = modelWriter.Length;
            var headerOffset = modelWriter.Length + sizeof(int);
            int size = 0;
            modelWriter.Write(0);
            modelWriter.WriteBytes(UnsafeUtility.AddressOf(ref header), UnsafeUtility.SizeOf<HeaderConfig>());
            foreach (var child in node.children) {
                modelWriter.Write(child);
            }

            size += UnsafeUtility.SizeOf<HeaderConfig>() + (UnsafeUtility.SizeOf<int>() * node.children.Count);
            size += ConfigureBlocks(node, modelWriter, decomposer,ref context, types, size, out header.flags);
            UnsafeUtility.MemCpy((((IntPtr)modelWriter.Data) + sizeOffset).ToPointer(), UnsafeUtility.AddressOf(ref size), UnsafeUtility.SizeOf<int>());
            UnsafeUtility.MemCpy((((IntPtr)modelWriter.Data) + headerOffset).ToPointer(), UnsafeUtility.AddressOf(ref header), UnsafeUtility.SizeOf<HeaderConfig>());
            totalSize += UnsafeUtility.SizeOf<int>() + size;
            foreach (var child in node.children) {
                Configure(model,index,child, schema, modelWriter, decomposer, context, types, ref totalSize);
            }
        }
        private static int ConfigureBlocks(UIModel.Node node, MemoryBinaryWriter writer, TypeDecomposer decomposer, ref UIPropertyWriterContext context, List<Type> types, int headerSize, out byte flags) {
            var configBlocks = new List<object>();
            UIConfigUtility.GetTypes(node.mask, types);
            UIConfigUtility.CreateConfiguration(node.mask, configBlocks);
            var configSize = UIConfigUtility.GetLength(node.mask);
            using var extraBytesStream = new MemoryBinaryWriter();
            IntPtr configData = (IntPtr)UnsafeUtility.Malloc(configSize, 0, Allocator.Temp);
            int configBlockOffset = 0;
            var configFields = new Dictionary<string, TypeDecomposer.FieldData>();
            flags = 0;
            foreach (var configBlock in configBlocks) {
                decomposer.Decompose(configBlock.GetType(), configFields, configBlock.GetType().GetCustomAttribute<UIConfigBlockAttribute>()?.Name, configBlockOffset, '-');
                Marshal.StructureToPtr(configBlock, configData + configBlockOffset, true);
                flags |= StandardConfigurationHandlers.PreInit(configBlock.GetType(), configData + configBlockOffset, node.mask,ref context);
                configBlockOffset += UnsafeUtility.SizeOf(configBlock.GetType());
            }
            foreach (var property in node.properties) {
                if (configFields.TryGetValue(property.path, out TypeDecomposer.FieldData fieldData)) {
                    StandardPropertyWriters.writers.Write(property.Value, configData, fieldData, extraBytesStream, configSize + headerSize, context);
                }
            }
            configBlockOffset = 0;
            foreach (var configBlockType in types) {
                flags |= StandardConfigurationHandlers.PostInit(configBlockType, configData + configBlockOffset, configData, node.mask, extraBytesStream, configSize + headerSize, context);
                configBlockOffset += UnsafeUtility.SizeOf(configBlockType);
            }
            int length = configSize + extraBytesStream.Length;
            writer.WriteBytes(configData.ToPointer(), configSize);
            if (extraBytesStream.Length > 0) {
                writer.WriteBytes(extraBytesStream.Data, extraBytesStream.Length);
            }
            UnsafeUtility.Free(configData.ToPointer(), Allocator.Temp);
            return length;
        }
    }
}