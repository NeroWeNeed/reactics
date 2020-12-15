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
using UnityEditor;
using UnityEngine;
namespace NeroWeNeed.UIDots {
    public class UIModel : ScriptableObject {
        [HideInInspector]
        public List<string> assets = new List<string>();
        [HideInInspector]
        public List<Node> nodes = new List<Node>();
        public string spriteGroupName;
        public void CollectTextures(out Texture2D atlas, out Texture2D[] fonts) {
            var spriteGroup = UISpriteGroup.Find(spriteGroupName);
            atlas = spriteGroup?.GenerateTexture();
            fonts = this.assets.Select(a => AssetDatabase.GUIDToAssetPath(a)).Where(a => a != null && AssetDatabase.GetMainAssetTypeAtPath(a) == typeof(TMP_FontAsset))
            .Select(a => Array.Find(AssetDatabase.LoadAllAssetRepresentationsAtPath(a), b => b is Texture2D)).Where(a => a != null).OfType<Texture2D>().ToArray();
        }



        public unsafe BlobAssetReference<UIGraph> Create(Allocator blobAllocator = Allocator.TempJob) {
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref UIGraph graph = ref builder.ConstructRoot<UIGraph>();
            var schema = UISchema.Default;
            using var configurationWriter = new MemoryBinaryWriter();
            var context = new UIPropertyWriterContext
            {
                spriteGroup = UISpriteGroup.Find(spriteGroupName),
                fonts = this.assets.Where(a => AssetDatabase.GetMainAssetTypeAtPath(a) == typeof(TMP_FontAsset)).ToArray()
            };
            var configInfo = new List<ConfigData>();
            var decomposer = new TypeDecomposer();
            for (int i = 0; i < nodes.Count; i++) {
                configInfo.Add(Configure(nodes[i], configurationWriter, schema, decomposer, context));
            }
            var nodeBuilder = builder.Allocate<UIGraphNode>(ref graph.nodes, nodes.Count);
            for (int i = 0; i < nodes.Count; i++) {

                nodeBuilder[i] = new UIGraphNode
                {
                    pass = BurstCompiler.CompileFunctionPointer<UIPass>(nodes[i].pass.Value.CreateDelegate(typeof(UIPass)) as UIPass)
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
        private unsafe static ConfigData Configure(UIModel.Node node, MemoryBinaryWriter writer, UISchema schema, TypeDecomposer decomposer, UIPropertyWriterContext context) {
            var element = schema.Entries[node.identifier];
            var configHeader = UIConfig.DEFAULT;
            var extraConfigType = element.config.Value;
            var configSize = UnsafeUtility.SizeOf<UIConfig>();
            void* configData;

            using var extraBytesStream = new MemoryBinaryWriter();
            if (extraConfigType != null) {
                configSize += UnsafeUtility.SizeOf(extraConfigType);
                var extraConfigObj = Activator.CreateInstance(extraConfigType);
                if (extraConfigObj is IInitializable initializable)
                    initializable.PreInit(configHeader, extraBytesStream, configSize, context);
                configData = UnsafeUtility.Malloc(configSize, 0, Allocator.Temp);
                UnsafeUtility.CopyStructureToPtr(ref configHeader, configData);
                Marshal.StructureToPtr(extraConfigObj, (IntPtr)configData + UnsafeUtility.SizeOf<UIConfig>(), false);
            }
            else {
                configData = UnsafeUtility.Malloc(configSize, 0, Allocator.Temp);
                UnsafeUtility.CopyStructureToPtr(ref configHeader, configData);
            }

            var baseConfigFields = decomposer.Decompose(typeof(UIConfig), '-');

            if (extraConfigType != null) {
                var extraConfigFields = decomposer.Decompose(extraConfigType, '-');
                TypeDecomposer.FieldData fieldData;
                foreach (var property in node.properties) {

                    if (baseConfigFields.TryGetValue(property.path, out fieldData)) {
                        StandardPropertyWriters.writers.Write(property.Value, (IntPtr)configData, fieldData, extraBytesStream, configSize, context);
                    }
                    else if (extraConfigFields.TryGetValue(property.path, out fieldData)) {
                        StandardPropertyWriters.writers.Write(property.Value, (IntPtr)configData, new TypeDecomposer.FieldData(fieldData, UnsafeUtility.SizeOf<UIConfig>()), extraBytesStream, configSize, context);
                    }
                }
                UnsafeUtility.CopyPtrToStructure<UIConfig>(configData, out UIConfig c);
                var t = Marshal.PtrToStructure(((IntPtr)configData) + UnsafeUtility.SizeOf<UIConfig>(), extraConfigType);
                if (t is IInitializable initializable) {
                    initializable.PostInit(c, extraBytesStream, configSize, context);
                    Marshal.StructureToPtr(t, ((IntPtr)configData) + UnsafeUtility.SizeOf<UIConfig>(), true);
                }
            }
            else {
                foreach (var property in node.properties) {
                    if (baseConfigFields.TryGetValue(property.path, out TypeDecomposer.FieldData fieldData)) {
                        StandardPropertyWriters.writers.Write(property.Value, (IntPtr)configData, fieldData, extraBytesStream, configSize, context);
                    }
                }
            }
            var config = new ConfigData
            {
                offset = writer.Length
            };
            writer.Write(configSize + extraBytesStream.Length);
            writer.WriteBytes(configData, configSize);

            if (extraBytesStream.Length > 0) {
                writer.WriteBytes(extraBytesStream.Data, extraBytesStream.Length);
            }
            UnsafeUtility.Free(configData, Allocator.Temp);
            config.length = writer.Length - config.length;
            return config;


        }
        private void OnDestroy() {
            UISpriteGroup.Find(spriteGroupName)?.Remove(this, this.assets);

        }
        [Serializable]
        public struct Node {
            public string identifier;
            public string name;
            public SerializableMethod pass;
            public List<Property> properties;
            public List<int> children;
            public int parent;

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