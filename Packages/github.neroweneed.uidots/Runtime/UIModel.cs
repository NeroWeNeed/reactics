using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using NeroWeNeed.Commons;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
namespace NeroWeNeed.UIDots {
    public class UIModel : ScriptableObject {
        [HideInInspector]
        public List<string> assets = new List<string>();
        [HideInInspector]
        public List<Node> nodes = new List<Node>();
        public string groupName;
        #if UNITY_EDITOR
        public Material GetMaterial() => UIAssetGroup.Find(groupName)?.UpdateMaterial();

        public unsafe BlobAssetReference<UIGraph> Create(Allocator blobAllocator = Allocator.TempJob) {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref UIGraph graph = ref builder.ConstructRoot<UIGraph>();
            var schema = UISchema.Default;
            using var configurationWriter = new MemoryBinaryWriter();
            var assetGroup = UIAssetGroup.Find(groupName);
            var context = new UIPropertyWriterContext
            {
                group = assetGroup
            };
            var configInfo = new List<ConfigData>();
            var decomposer = new TypeDecomposer();
            var types = new List<Type>();
            for (int i = 0; i < nodes.Count; i++) {
                configInfo.Add(Configure(nodes[i], configurationWriter, schema, decomposer, context, types));
            }
            var nodeBuilder = builder.Allocate<UIGraphNode>(ref graph.nodes, nodes.Count);
            for (int i = 0; i < nodes.Count; i++) {

                nodeBuilder[i] = new UIGraphNode
                {
                    pass = BurstCompiler.CompileFunctionPointer<UIPass>(nodes[i].pass.Value.CreateDelegate(typeof(UIPass)) as UIPass),
                    configurationMask = nodes[i].mask
                };
                var renderBoxHandlerAttr = nodes[i].pass.Value.GetCustomAttribute<UIDotsRenderBoxHandlerAttribute>();
                if (renderBoxHandlerAttr != null) {
                    var handlerMethod = nodes[i].pass.Value.DeclaringType.GetMethod(renderBoxHandlerAttr.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    if (handlerMethod?.GetCustomAttribute<BurstCompileAttribute>() != null) {
                        nodeBuilder[i].renderBoxHandler = BurstCompiler.CompileFunctionPointer<UIRenderBoxHandler>(handlerMethod.CreateDelegate(typeof(UIRenderBoxHandler)) as UIRenderBoxHandler);
                    }
                }
                var nestedChildrenBuilder = builder.Allocate<int>(ref nodeBuilder[i].children, nodes[i].children.Count);
                for (int j = 0; j < nodes[i].children.Count; j++)
                    nestedChildrenBuilder[j] = nodes[i].children[j];
            }
            var initialConfiguration = builder.Allocate(ref graph.initialConfiguration, configurationWriter.Length);
            UnsafeUtility.MemCpy(initialConfiguration.GetUnsafePtr(), configurationWriter.Data, configurationWriter.Length);
            var asset = builder.CreateBlobAssetReference<UIGraph>(blobAllocator);
            builder.Dispose();
            return asset;

        }
        private unsafe static void PostInitConfigBlock<TConfig>(IntPtr configData, ulong mask, int offset, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) where TConfig : struct, IConfig {
            var configBlock = UnsafeUtility.AsRef<TConfig>((configData + offset).ToPointer());
            configBlock.PostInit(configData, mask, extraBytesStream, extraByteStreamOffset, context);
            UnsafeUtility.CopyStructureToPtr(ref configBlock, (configData + offset).ToPointer());
        }
        private unsafe static void PostInitConfigBlockTypeless(Type type, IntPtr configData, ulong mask, int offset, MemoryBinaryWriter extraBytesStream, int extraByteStreamOffset, UIPropertyWriterContext context) {
            typeof(UIModel).GetMethod(nameof(PostInitConfigBlock), BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).MakeGenericMethod(type).Invoke(null, new object[] { configData, mask, offset, extraBytesStream, extraByteStreamOffset, context });
        }
        private unsafe static ConfigData Configure(UIModel.Node node, MemoryBinaryWriter writer, UISchema schema, TypeDecomposer decomposer, UIPropertyWriterContext context, List<Type> types) {
            var element = schema.Entries[node.identifier];
            UIConfigLayout.GetTypes(element.mask, types);
            var configBlocks = new List<IConfig>();
            UIConfigLayout.CreateConfiguration(element.mask, configBlocks);
            var configSize = UIConfigLayout.GetLength(element.mask);
            using var extraBytesStream = new MemoryBinaryWriter();
            IntPtr configData = (IntPtr) UnsafeUtility.Malloc(configSize, 0, Allocator.Temp);
            int configOffset = 0;
            var configFields = new Dictionary<string, TypeDecomposer.FieldData>();
            foreach (var configBlock in configBlocks) {
                decomposer.Decompose(configBlock.GetType(), configFields, configBlock.GetType().GetCustomAttribute<UIConfigBlockAttribute>()?.Name,configOffset,'-');
                configBlock.PreInit(element.mask, context);
                Marshal.StructureToPtr(configBlock, configData + configOffset, true);
                configOffset += UnsafeUtility.SizeOf(configBlock.GetType());
            }

            foreach (var property in node.properties) {
                if (configFields.TryGetValue(property.path, out TypeDecomposer.FieldData fieldData)) {
                    StandardPropertyWriters.writers.Write(property.Value, configData, fieldData, extraBytesStream, configSize, context);
                }
            }
            configOffset = 0;
            foreach (var configBlockType in types) {
                PostInitConfigBlockTypeless(configBlockType, configData, element.mask, configOffset, extraBytesStream, configSize, context);
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

        private void OnDestroy() {
            UIAssetGroup.Find(groupName)?.Remove(this, this.assets);

        }
        #endif
        [Serializable]
        public struct Node {
            public string identifier;
            public string name;
            public SerializableMethod pass;
            public List<Property> properties;
            public List<int> children;
            public int parent;
            public ulong mask;

            [Serializable]
            public struct Property {
                public string path;
                public string value;
                public string Path { get => path; }
                public string Value { get => value; }

            }
        }
        public struct ConfigData {
            public int offset;
            public int length;
        }
    }
}