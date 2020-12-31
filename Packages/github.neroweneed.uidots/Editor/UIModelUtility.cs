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
using UnityEngine;

namespace NeroWeNeed.UIDots.Editor {
    public unsafe static class UIModelUtility {
        public static Material GetMaterial(this UIModel model) => model.group?.Material;
        public static void Write(this Stream stream, UIModel model) {

        }
        public static BlobAssetReference<UIGraph> CreateGraphAsset(this UIModel model, Allocator allocator) {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref UIGraph graph = ref builder.ConstructRoot<UIGraph>();
            var schema = UISchema.Default;
            using var configurationWriter = new MemoryBinaryWriter();
            Contract.Assert(model.group != null);
            var context = new UIPropertyWriterContext
            {
                group = model.group
            };
            var configInfo = new List<ConfigData>();
            var decomposer = new TypeDecomposer();
            var types = new List<Type>();
            for (int i = 0; i < model.nodes.Count; i++) {
                configInfo.Add(Configure(model.nodes[i], configurationWriter, schema, decomposer, context, types));
            }
            var nodeBuilder = builder.Allocate<UIGraphNode>(ref graph.nodes, model.nodes.Count);
            for (int i = 0; i < model.nodes.Count; i++) {

                nodeBuilder[i] = new UIGraphNode
                {
                    pass = BurstCompiler.CompileFunctionPointer<UIPass>(model.nodes[i].pass.Value.CreateDelegate(typeof(UIPass)) as UIPass),
                    configurationMask = model.nodes[i].mask
                };
                var renderBoxHandlerAttr = model.nodes[i].pass.Value.GetCustomAttribute<UIDotsRenderBoxHandlerAttribute>();
                if (renderBoxHandlerAttr != null) {
                    var handlerMethod = model.nodes[i].pass.Value.DeclaringType.GetMethod(renderBoxHandlerAttr.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (handlerMethod?.GetCustomAttribute<BurstCompileAttribute>() != null) {
                        nodeBuilder[i].renderBoxHandler = BurstCompiler.CompileFunctionPointer<UIRenderBoxHandler>(handlerMethod.CreateDelegate(typeof(UIRenderBoxHandler)) as UIRenderBoxHandler);
                    }
                }
                var nestedChildrenBuilder = builder.Allocate<int>(ref nodeBuilder[i].children, model.nodes[i].children.Count);
                for (int j = 0; j < model.nodes[i].children.Count; j++)
                    nestedChildrenBuilder[j] = model.nodes[i].children[j];
            }
            var initialConfiguration = builder.Allocate(ref graph.initialConfiguration, configurationWriter.Length);
            UnsafeUtility.MemCpy(initialConfiguration.GetUnsafePtr(), configurationWriter.Data, configurationWriter.Length);
            var asset = builder.CreateBlobAssetReference<UIGraph>(allocator);
            builder.Dispose();
            return asset;
        }

        private static ConfigData Configure(UIModel.Node node, MemoryBinaryWriter writer, UISchema schema, TypeDecomposer decomposer, UIPropertyWriterContext context, List<Type> types) {
            var configBlocks = new List<object>();
            var element = schema.entries.Find(x => x.identifier == node.identifier);
            UIConfigUtility.GetTypes(element.mask, types);
            UIConfigUtility.CreateConfiguration(element.mask, configBlocks);
            var configSize = UIConfigUtility.GetLength(element.mask);
            using var extraBytesStream = new MemoryBinaryWriter();
            IntPtr configData = (IntPtr)UnsafeUtility.Malloc(configSize, 0, Allocator.Temp);
            int configOffset = 0;
            var configFields = new Dictionary<string, TypeDecomposer.FieldData>();
            foreach (var configBlock in configBlocks) {
                decomposer.Decompose(configBlock.GetType(), configFields, configBlock.GetType().GetCustomAttribute<UIConfigBlockAttribute>()?.Name, configOffset, '-');
                Marshal.StructureToPtr(configBlock, configData + configOffset, true);
                StandardConfigurators.PreInit(configBlock.GetType(), configData + configOffset, element.mask, context);
                configOffset += UnsafeUtility.SizeOf(configBlock.GetType());
            }
            foreach (var property in node.properties) {
                if (configFields.TryGetValue(property.path, out TypeDecomposer.FieldData fieldData)) {
                    StandardPropertyWriters.writers.Write(property.Value, configData, fieldData, extraBytesStream, configSize, context);
                }
            }
            configOffset = 0;
            foreach (var configBlockType in types) {
                StandardConfigurators.PostInit(configBlockType, configData + configOffset, configData, element.mask, extraBytesStream, configSize, context);
                configOffset += UnsafeUtility.SizeOf(configBlockType);
            }
            var config = new ConfigData
            {
                offset = writer.Length
            };
            writer.Write(configSize + extraBytesStream.Length);
            writer.WriteBytes(configData.ToPointer(), configSize);
            if (extraBytesStream.Length > 0) {
                writer.WriteBytes(extraBytesStream.Data, extraBytesStream.Length);
            }
            UnsafeUtility.Free(configData.ToPointer(), Allocator.Temp);
            config.length = writer.Length - config.length;
            return config;
        }
        public struct ConfigData {
            public int offset;
            public int length;
        }
    }
}