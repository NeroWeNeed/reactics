using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
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
    public unsafe static class UIModelExtensions {

        //TODO: Global Settings
        public const string UI_ADDRESSABLES_GROUP = "User Interfaces";
        public const string UI_OUTPUT_PATH = "Assets/ResourceData/UI";
        private static AddressableAssetGroup Group { get => AddressableAssetSettingsDefaultObject.Settings.FindGroup(UI_ADDRESSABLES_GROUP) ?? AddressableAssetSettingsDefaultObject.Settings.CreateGroup(UI_ADDRESSABLES_GROUP, false, false, false, null); }
        public static Material GetMaterial(this UIModel model) => model.group?.Material;
        public static string GetOutputGuid(this UIModel model) => AssetDatabase.GUIDFromAssetPath(model.output).ToString();
        
        public static string Write(this UIModel model) {
            var fi = new FileInfo(model.output);
            fi.Directory.Create();
            using (var fs = fi.Create()) {
                Write(model, fs);
            }
            AssetDatabase.ImportAsset(model.output);
            return AssetDatabase.GUIDFromAssetPath(model.output).ToString();

            /* var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(AssetDatabase.GUIDFromAssetPath(model.output).ToString(), Group);
            Debug.Log(entry.guid);
            if (!string.IsNullOrEmpty(model.address)) {
                entry.SetAddress(model.address);
            }
            return entry.guid; */
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
        public static void Write(this UIModel model, Stream stream) {
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
            for (int i = 0; i < model.nodes.Count; i++) {
                totalSize += Configure(model.nodes[i], modelWriter, decomposer, context, types) + UnsafeUtility.SizeOf<int>();
            }
            UnsafeUtility.MemCpy(modelWriter.Data, UnsafeUtility.AddressOf(ref totalSize), UnsafeUtility.SizeOf<ulong>());
            using var modelStream = new UnmanagedMemoryStream(modelWriter.Data, modelWriter.Length);
            modelStream.CopyTo(stream);
        }
        private static int Configure(UIModel.Node node, MemoryBinaryWriter writer, TypeDecomposer decomposer, UIPropertyWriterContext context, List<Type> types) {
            var header = new HeaderConfig
            {
                configurationMask = node.mask,
                layoutPass = BurstCompiler.CompileFunctionPointer(node.layoutPass.Value.CreateDelegate(typeof(UILayoutPass)) as UILayoutPass),
                renderPass = BurstCompiler.CompileFunctionPointer(node.renderPass.Value.CreateDelegate(typeof(UIRenderPass)) as UIRenderPass),
                renderBoxCounter = node.renderBoxCounter.IsCreated ? BurstCompiler.CompileFunctionPointer<UIRenderBoxCounter>(node.renderBoxCounter.Value.CreateDelegate(typeof(UIRenderBoxCounter)) as UIRenderBoxCounter) : default,
                flags = 0,
                childCount = node.children.Count
            };
            var sizeOffset = writer.Length;
            var headerOffset = writer.Length + sizeof(int);
            int size = 0;
            writer.Write(0);
            writer.WriteBytes(UnsafeUtility.AddressOf(ref header), UnsafeUtility.SizeOf<HeaderConfig>());
            foreach (var child in node.children) {
                writer.Write(child);
            }
            size += UnsafeUtility.SizeOf<HeaderConfig>() + (UnsafeUtility.SizeOf<int>() * node.children.Count);
            size += ConfigureBlocks(node, writer, decomposer, context, types, size, out header.flags);
            UnsafeUtility.MemCpy((((IntPtr)writer.Data) + sizeOffset).ToPointer(), UnsafeUtility.AddressOf(ref size), UnsafeUtility.SizeOf<int>());
            UnsafeUtility.MemCpy((((IntPtr)writer.Data) + headerOffset).ToPointer(), UnsafeUtility.AddressOf(ref header), UnsafeUtility.SizeOf<HeaderConfig>());
            return size;
        }
        private static int ConfigureBlocks(UIModel.Node node, MemoryBinaryWriter writer, TypeDecomposer decomposer, UIPropertyWriterContext context, List<Type> types, int headerSize, out byte flags) {
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
                flags |= StandardConfigurationHandlers.PreInit(configBlock.GetType(), configData + configBlockOffset, node.mask, context);
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