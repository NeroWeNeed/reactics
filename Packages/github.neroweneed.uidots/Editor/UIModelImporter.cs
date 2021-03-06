using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using static NeroWeNeed.UIDots.TypeDecomposer;

namespace NeroWeNeed.UIDots.Editor {
    [ScriptedImporter(1, "uidml")]
    public class UIModelImporter : ScriptedImporter {
        public const string ROOT_ELEMENT = "UIGraph";
        public const string GROUP_ATTRIBUTE = "group";
        public const string ADDRESS_ATTRIBUTE = "address";
        public const string OUTPUT_ATTRIBUTE = "output";
        public const string DEFAULT_GROUP_NAME = "default";
/*         [UnityEditor.Callbacks.DidReloadScripts]
        [InitializeOnLoadMethod] */
        private static void DirtyModelsOnScriptReload() {
            foreach (var modelGuid in AssetDatabase.FindAssets($"t:{nameof(UIModel)}")) {
                var model = AssetDatabase.LoadAssetAtPath<UIModel>(AssetDatabase.GUIDToAssetPath(modelGuid));
                if (model != null)
                    model.Write();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public override void OnImportAsset(AssetImportContext ctx) {
            var model = ScriptableObject.CreateInstance<UIModel>();
            InitializeModel(model, new StringReader(File.ReadAllText(ctx.assetPath)), AssetDatabase.GUIDFromAssetPath(ctx.assetPath).ToString(), ctx.assetPath);
            model.Write();
            ctx.AddObjectToAsset("UI Model", model);
            ctx.SetMainObject(model);
        }
        private static bool IsRoot(XmlReader reader) => reader.NodeType == XmlNodeType.Element && reader.Name == ROOT_ELEMENT;
        private static void InitializeModel(UIModel model, StringReader reader, string guid, string path) {
            using XmlReader xmlReader = XmlReader.Create(reader);
            InitializeModel(model, xmlReader, guid, path);
        }
        private static void InitializeModel(UIModel model, XmlReader reader, string guid, string path) {
            var settings = UIGlobalSettings.GetOrCreateSettings();
            var referencedAssets = new List<string>();
            var nodes = new List<UIGraphNodeOld>();
            var context = new Context();
            Header header = default;
            TypeDecomposer decomposer = new TypeDecomposer();
            if (reader.Read() && IsRoot(reader)) {
                header = new Header(settings, reader, model);
                var childReader = reader.ReadSubtree();
                var root = Node.Create(default, -1);
                while (childReader.Read()) {
                    Parse(reader, root, -1, 0, settings, context, decomposer);
                }
            }
            if (context.nodes.Count > 0) {
                model.assets.AddRange(context.nodes.SelectMany(xmlNode => xmlNode.assetReferences).Distinct());
                model.group = UIAssetGroup.Find(header.groupName);
                model.output = header.output;
                model.address = header.address;
                if (model.group != null) {
                    var textures = model.assets.Where(a =>
                    {
                        var type = AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(a));
                        return type == typeof(Texture2D) || type == typeof(TMP_FontAsset);
                    }).ToArray();
                    model.group.Add(guid, textures);
                    EditorUtility.SetDirty(model.group);
                }
                model.assets.Sort();
                model.nodes.AddRange(context.nodes.Select(node => node.ToNode()));
            }
        }
        private unsafe static void Parse(XmlReader reader, Node parent, int parentIndex, int depth, UIGlobalSettings settings, Context context, TypeDecomposer decomposer) {
            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element && settings.Elements.TryGetValue(reader.Name, out UISchema.Element element)) {
                    var node = Node.Create(element, parentIndex);
                    ulong mask = element.requiredBlockMask;
                    if (reader.HasAttributes) {
                        UIConfigUtility.GetTypes(element.optionalBlockMask, context.optionalTypeBuffer);
                        var optionalPaths = new Dictionary<string, Type>();
                        
                        foreach (var type in context.optionalTypeBuffer) {
                            foreach (var path in decomposer.DecomposePath(type, '-')) {
                                optionalPaths[path] = type;
                            }
                        }
                        for (int i = 0; i < reader.AttributeCount; i++) {
                            reader.MoveToAttribute(i);
                            node.properties[reader.Name] = reader.Value;
                            if (optionalPaths.TryGetValue(reader.Name, out Type configType)) {
                                Debug.Log(UIConfigUtility.GetMask(configType));
                                mask |= UIConfigUtility.GetMask(configType);
                            }
                        }
                        reader.MoveToElement();
                        UIConfigUtility.GetTypes(mask, context.maskBuffer);

                        var fields = new Dictionary<string, FieldData>();
                        var configSize = 0;

                        foreach (var type in context.maskBuffer) {
                            decomposer.Decompose(type, fields, type.GetCustomAttribute<UIConfigBlockAttribute>()?.Name, configSize, '-');
                            configSize += UnsafeUtility.SizeOf(type);
                        }
                        foreach (var key in node.properties.Keys.ToArray()) {
                            var value = node.properties[key];
                            if (fields.TryGetValue(key, out FieldData data) && data.isAssetReference) {
                                var guid = value.StartsWith("guid:") ? value.Substring(5) : AssetDatabase.GUIDFromAssetPath(value).ToString();
                                if (!node.assetReferences.Contains(guid))
                                    node.assetReferences.Add(guid);
                                node.properties[key] = guid;
                            }
                        }
                    }
                    node.mask = mask;
                    var index = context.nodes.Count;
                    parent.children.Add(index);
                    context.nodes.Add(node);
                    var subtree = reader.ReadSubtree();
                    subtree.Read();
                    Parse(subtree, node, index, depth + 1, settings, context, decomposer);
                    subtree.Close();
                }
            }
        }
        private class Context {
            public List<Node> nodes = new List<Node>();
            public List<Type> maskBuffer = new List<Type>();
            public List<Type> optionalTypeBuffer = new List<Type>();
        }
        private struct Header {
            public string groupName;
            public string output;
            public string address;
            public Header(UIGlobalSettings settings, XmlReader reader, UIModel model) {
                var hasAttributes = reader.HasAttributes;
                groupName = hasAttributes && reader.MoveToAttribute(GROUP_ATTRIBUTE) ? reader.Value : DEFAULT_GROUP_NAME;
                address = hasAttributes && reader.MoveToAttribute(ADDRESS_ATTRIBUTE) ? reader.Value : null;
                output = hasAttributes && reader.MoveToAttribute(OUTPUT_ATTRIBUTE) ? reader.Value : $"{settings.outputPath}/{model.name}.bytes";
                if (!output.EndsWith(".bytes"))
                    output += ".bytes";
                if (hasAttributes)
                    reader.MoveToElement();
            }
        }
        private struct Node {
            public ulong mask;
            public Dictionary<string, string> properties;
            public UISchema.Element element;
            public List<int> children;
            public List<string> assetReferences;
            public int parent;
            public static Node Create(UISchema.Element element, int parentIndex) => new Node { children = new List<int>(), properties = new Dictionary<string, string>(), element = element, parent = parentIndex, assetReferences = new List<string>() };
            public UIModel.Node ToNode() {
                return new UIModel.Node
                {
                    identifier = element.identifier,
                    layoutPass = element.layoutPass,
                    renderPass = element.renderPass,
                    renderBoxCounter = element.renderBoxCounter,
                    properties = properties?.Select(property => new UIModel.Node.Property { path = property.Key, value = property.Value })?.ToList(),
                    children = children,
                    parent = parent,
                    mask = mask
                };
            }
        }
    }
}