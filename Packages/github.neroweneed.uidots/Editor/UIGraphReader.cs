using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using NeroWeNeed.BehaviourGraph.Editor;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Sprites;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using static NeroWeNeed.UIDots.TypeDecomposer;
using static NeroWeNeed.UIDots.UIModel;

[assembly: SearchableAssembly]
namespace NeroWeNeed.UIDots.Editor {


    public static class UIGraphReader {
        public const string ROOT_ELEMENT = "UIGraph";
        public const string GROUP_ATTRIBUTE = "group";
        public const string CALLBACK_CONTAINER_ELEMENT = "Callbacks";
        public const string CALLBACK_ELEMENT = "Callback";

        public const string CALLBACK_TYPE_ATTRIBUTE = "type";

        public const string CALLBACK_NAME_ATTRIBUTE = "name";

        public static void InitModel(UIModel model, StringReader reader, string guid) {
            using XmlReader xmlReader = XmlReader.Create(reader);
            InitModel(xmlReader, model, guid);
        }
        private static void InitModel(XmlReader reader, UIModel model, string guid) {
            model.groupName = "default";
            var referencedAssets = new List<string>();
            var nodes = new List<UIGraphNode>();
            var context = new UIGraphContext();
            var schema = UISchema.Default;
            
            TypeDecomposer decomposer = new TypeDecomposer();
            if (reader.Read() && IsRoot(reader)) {
                if (reader.HasAttributes) {
                    if (reader.MoveToAttribute(GROUP_ATTRIBUTE)) {
                        model.groupName = reader.Value;
                    }
                    reader.MoveToElement();
                }
                var childReader = reader.ReadSubtree();
                var root = UIGraphXmlNode.Create(null, default, -1);

                while (childReader.Read()) {
                    Parse(reader, root, -1, 0, schema, context, decomposer);
                }
            }
            model.assets.AddRange(context.nodes.SelectMany(xmlNode => xmlNode.assetReferences).Distinct());
            var group = UIAssetGroup.Find(model.groupName);
            if (group != null) {
                var textures = model.assets.Where(a =>
                {
                    var type = AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(a));
                    return type == typeof(Texture2D) || type == typeof(TMP_FontAsset);
                }).ToArray();
                group.Add(guid, textures);
                EditorUtility.SetDirty(group);
            }
            model.assets.Sort();
            model.nodes.AddRange(context.nodes.Select(node => node.ToNode()));
        }
        private static bool IsRoot(XmlReader reader) => reader.NodeType == XmlNodeType.Element && reader.Name == ROOT_ELEMENT;
        public unsafe static void Parse(XmlReader reader, UIGraphXmlNode parent, int parentIndex, int depth, UISchema schema, UIGraphContext context, TypeDecomposer decomposer) {
            while (reader.Read()) {
                /* if (reader.NodeType == XmlNodeType.Element && reader.Name == CALLBACK_CONTAINER_ELEMENT) {

                    var callbackContainers = new List<CallbackContainer>();
                    var callbackContainerDefinitionReader = reader.ReadSubtree();
                    callbackContainerDefinitionReader.Read();
                    while (callbackContainerDefinitionReader.Read()) {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == CALLBACK_ELEMENT) {
                            if (!reader.MoveToAttribute(CALLBACK_TYPE_ATTRIBUTE)) {
                                continue;
                            }
                            Type type = Type.GetType(reader.Value);
                            string name;
                            if (reader.MoveToAttribute(CALLBACK_NAME_ATTRIBUTE)) {
                                name = reader.Value;
                            }
                            else {
                                name = type.FullName;
                            }
                            reader.MoveToElement();
                            callbackContainers.Add(new CallbackContainer(name,new SerializableType(type)));
                        }

                    }
                    context.callbackContainerDefinitions.AddRange(callbackContainers);

                }
                else  */
                if (reader.NodeType == XmlNodeType.Element && schema.Entries.TryGetValue(reader.Name, out UISchema.Element element)) {
                    var node = UIGraphXmlNode.Create(reader.Name, element, parentIndex);
                    if (reader.HasAttributes) {
                        UIConfigLayout.GetTypes(element.mask, context.typeBuffer);
                        var fields = new Dictionary<string, FieldData>();
                        var configSize = 0;
                        foreach (var type in context.typeBuffer)
                        {
                            decomposer.Decompose(type, fields, type.GetCustomAttribute<UIConfigBlockAttribute>()?.Name,configSize, '-');
                            configSize += UnsafeUtility.SizeOf(type);
                        }
                        for (int i = 0; i < reader.AttributeCount; i++) {
                            reader.MoveToAttribute(i);
                            node.properties[reader.Name] = reader.Value;
                            if (fields.TryGetValue(reader.Name, out TypeDecomposer.FieldData data) && data.isAssetReference) {
                                var guid = reader.Value.StartsWith("guid:") ? reader.Value.Substring(5) : AssetDatabase.GUIDFromAssetPath(reader.Value).ToString();
                                if (!node.assetReferences.Contains(guid))
                                    node.assetReferences.Add(guid);
                                node.properties[reader.Name] = guid;
                            }

                        }
                            reader.MoveToElement();
                    }
                    node.pass = node.element.pass.Value;
                    var index = context.nodes.Count;
                    parent.children.Add(index);
                    context.nodes.Add(node);

                    var subtree = reader.ReadSubtree();
                    subtree.Read();
                    Parse(subtree, node, index, depth + 1, schema, context, decomposer);
                    subtree.Close();

                }
            }
        }



    }
    public class UIGraphContext {
        public List<UIGraphXmlNode> nodes = new List<UIGraphXmlNode>();

        public List<Type> typeBuffer = new List<Type>();
    }

    public struct UIGraphXmlNode {
        public string identifier;
        public string name;
        public int configurationOffset;
        public int configurationLength;
        public MethodInfo pass;
        public Dictionary<string, string> properties;
        public UISchema.Element element;
        public List<int> children;
        public List<string> assetReferences;
        public int parent;
        public static UIGraphXmlNode Create(string identifier, UISchema.Element element, int parentIndex) => new UIGraphXmlNode { identifier = identifier, children = new List<int>(), properties = new Dictionary<string, string>(), element = element, parent = parentIndex, assetReferences = new List<string>() };
        public override string ToString() {
            return string.IsNullOrEmpty(name) ? $"{identifier}" : $"{identifier} ({name})";
        }
        public NeroWeNeed.UIDots.UIModel.Node ToNode() {
            return new UIDots.UIModel.Node
            {
                identifier = identifier,
                name = name,
                pass = pass,
                properties = properties?.Select(property => new NeroWeNeed.UIDots.UIModel.Node.Property { path = property.Key, value = property.Value })?.ToList(),
                children = children,
                parent = parent,
                mask = element.mask
            };

        }
    }
}