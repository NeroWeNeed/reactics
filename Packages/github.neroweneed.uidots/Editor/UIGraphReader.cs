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
using UnityEditor.Sprites;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using static NeroWeNeed.UIDots.TypeDecomposer;


[assembly: SearchableAssembly]
namespace NeroWeNeed.UIDots.Editor {


    public static class UIGraphReader {
        public const string ROOT_ELEMENT = "UIGraph";
        public const string GROUP_ATTRIBUTE = "group";
        public const string ADDRESS_ATTRIBUTE = "address";

        internal static void Initialize(this UIModel model, StringReader reader, string guid,string path) {
            using XmlReader xmlReader = XmlReader.Create(reader);
            Initialize(model,xmlReader, guid,path);
        }
        internal static void Initialize(this UIModel model,XmlReader reader, string guid, string path) {

            var groupName = "default";
            var address = path.Substring(0,path.LastIndexOf('.'));
            var referencedAssets = new List<string>();
            var nodes = new List<UIGraphNode>();
            var context = new UIGraphContext();
            var schema = UISchema.Default;
            
            TypeDecomposer decomposer = new TypeDecomposer();
            if (reader.Read() && IsRoot(reader)) {
                if (reader.HasAttributes) {
                    if (reader.MoveToAttribute(GROUP_ATTRIBUTE)) {
                        groupName = reader.Value;
                    }
                    if (reader.MoveToAttribute(ADDRESS_ATTRIBUTE)) {
                        address = reader.Value;
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
            model.group = UIAssetGroup.Find(groupName);
            model.address = address;
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
        private static bool IsRoot(XmlReader reader) => reader.NodeType == XmlNodeType.Element && reader.Name == ROOT_ELEMENT;
        private unsafe static void Parse(XmlReader reader, UIGraphXmlNode parent, int parentIndex, int depth, UISchema schema, UIGraphContext context, TypeDecomposer decomposer) {
            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element && schema.Entries.TryGetValue(reader.Name, out UISchema.Element element)) {
                    var node = UIGraphXmlNode.Create(reader.Name, element, parentIndex);
                    if (reader.HasAttributes) {
                        UIConfigUtility.GetTypes(element.mask, context.typeBuffer);
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
        public SerializableMethod pass;
        public Dictionary<string, string> properties;
        public UISchema.Element element;
        public List<int> children;
        public List<string> assetReferences;
        public int parent;
        public static UIGraphXmlNode Create(string identifier, UISchema.Element element, int parentIndex) => new UIGraphXmlNode { identifier = identifier, children = new List<int>(), properties = new Dictionary<string, string>(), element = element, parent = parentIndex, assetReferences = new List<string>() };
        public override string ToString() {
            return string.IsNullOrEmpty(name) ? $"{identifier}" : $"{identifier} ({name})";
        }
        public UIModel.Node ToNode() {
            return new UIModel.Node
            {
                identifier = identifier,
                name = name,
                pass = pass,
                properties = properties?.Select(property => new UIModel.Node.Property { path = property.Key, value = property.Value })?.ToList(),
                children = children,
                parent = parent,
                mask = element.mask
            };

        }
    }
}