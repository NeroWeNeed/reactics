using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NeroWeNeed.Commons;
using NeroWeNeed.Commons.Editor;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace NeroWeNeed.UIDots.Editor {
    [ScriptedImporter(1, "uidml")]
    public class UIModelImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            try {
                var asset = ScriptableObject.CreateInstance<UIModel>();
                try {
                    UIGraphReader.InitModel(asset, new StringReader(File.ReadAllText(ctx.assetPath)));
                    ctx.AddObjectToAsset("UI Model", asset);
                    ctx.SetMainObject(asset);
                    if (asset.spriteTable != null) {
                        asset.spriteTable.AddModel(ctx.assetPath);
                        foreach (var sprite in asset.assets) {
                            asset.spriteTable.AddSprite(sprite);
                        }
                    }
                }
                catch (Exception e) {
                    Debug.LogError(e);
                }

            }
            catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
    public class UIModel : CompileableObject {
        [HideInInspector]
        public List<string> assets = new List<string>();
        [HideInInspector]
        public List<Node> nodes = new List<Node>();
        [HideInInspector]
        public UISpriteTable spriteTable;

        internal SpriteAtlas spriteAtlas;
        public override unsafe void Compile(CompileOptions hint = CompileOptions.None, bool forceCompilation = false) {
            this.spriteAtlas = null;
            BlobBuilder builder = new BlobBuilder(Allocator.Temp);
            ref UIGraph graph = ref builder.ConstructRoot<UIGraph>();
            var schema = UISchemaAsset.Schema;
            using var configurationWriter = new MemoryBinaryWriter();
            spriteTable?.BuildSpriteAtlas();
            var configInfo = new List<ConfigData>();
            var decomposer = new TypeDecomposer();
            for (int i = 0; i < nodes.Count; i++) {
                configInfo.Add(Configure(nodes[i], configurationWriter, schema, decomposer));
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

                builder.AllocateString(ref nodeBuilder[i].name, string.IsNullOrEmpty(nodes[i].name) ? string.Empty : nodes[i].name);
            }
            using var asset = builder.CreateBlobAssetReference<UIGraph>(Allocator.Temp);
            var outputAsset = ScriptableObject.CreateInstance<UICompiledModel>();
            using (var graphMemory = new MemoryBinaryWriter()) {
                graphMemory.Write(asset);
                outputAsset.name = this.name;

                outputAsset.graph = new byte[graphMemory.Length];
                Marshal.Copy((IntPtr)graphMemory.Data, outputAsset.graph, 0, graphMemory.Length);
            }
            outputAsset.initialConfiguration = new byte[configurationWriter.Length];
            Marshal.Copy((IntPtr)configurationWriter.Data, outputAsset.initialConfiguration, 0, configurationWriter.Length);
            AssetDatabase.CreateAsset(outputAsset, $"{outputDirectory}/{outputFileName}.asset");
            builder.Dispose();
        }

        private unsafe static ConfigData Configure(UIModel.Node node, MemoryBinaryWriter writer, UISchemaAsset schema, TypeDecomposer decomposer) {
            var element = schema.Elements[node.identifier];
            var configHeader = UIConfig.DEFAULT;
            var extraConfigType = element.config.Value;
            var configSize = UnsafeUtility.SizeOf<UIConfig>();
            void* configData;
            using var extraBytesStream = new MemoryBinaryWriter();
            if (extraConfigType != null) {
                configSize += UnsafeUtility.SizeOf(extraConfigType);
                var extraConfigObj = Activator.CreateInstance(extraConfigType);
                if (extraConfigObj is IInitializable initializable)
                    initializable.PreInit(configHeader, extraBytesStream, configSize);
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
                        StandardPropertyWriters.writers.Write(property.Value, (IntPtr)configData, fieldData, extraBytesStream, configSize);
                    }
                    else if (extraConfigFields.TryGetValue(property.path, out fieldData)) {
                        StandardPropertyWriters.writers.Write(property.Value, (IntPtr)configData, new TypeDecomposer.FieldData(fieldData, UnsafeUtility.SizeOf<UIConfig>()), extraBytesStream, configSize);
                    }
                }
                UnsafeUtility.CopyPtrToStructure<UIConfig>(configData, out UIConfig c);
                var t = Marshal.PtrToStructure(((IntPtr)configData) + UnsafeUtility.SizeOf<UIConfig>(), extraConfigType);
                if (t is IInitializable initializable) {
                    initializable.PostInit(c, extraBytesStream, configSize);
                    Marshal.StructureToPtr(t, ((IntPtr)configData) + UnsafeUtility.SizeOf<UIConfig>(), true);
                }
            }
            else {
                foreach (var property in node.properties) {
                    if (baseConfigFields.TryGetValue(property.path, out TypeDecomposer.FieldData fieldData)) {
                        StandardPropertyWriters.writers.Write(property.Value, (IntPtr)configData, fieldData, extraBytesStream, configSize);
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
        public static bool GetFontAsset(List<Node> nodes, int index, out TMP_FontAsset font) {
            int currentIndex = index;
            while (currentIndex >= 0) {
                var current = nodes[currentIndex];
                var p = current.properties.Find(p => p.path == "font");
                if (!EqualityComparer<Node.Property>.Default.Equals(p, default)) {
                    font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p.value.StartsWith("guid:") ? AssetDatabase.GUIDToAssetPath(p.value.Substring(5)) : p.value);
                    return true;
                }
                currentIndex = current.parent;
            }
            font = null;
            return false;

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

            public Node(UIGraphXmlNode node) {
                this.identifier = node.identifier;
                this.name = node.name;
                this.pass = node.pass;
                this.properties = node.properties?.Select(property => new Property { path = property.Key, value = property.Value })?.ToList();
                children = node.children;
                parent = node.parent;
            }
        }
        public struct ConfigData {
            public int offset;
            public int length;
        }

    }
}
