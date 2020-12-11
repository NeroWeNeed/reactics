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

[assembly: SearchableAssembly]
namespace NeroWeNeed.UIDots.Editor {

    public static class UIGraphReader {
        public const string ROOT_ELEMENT = "UIGraph";

        public static void InitModel(UIModel model, StringReader reader) {
            using XmlReader xmlReader = XmlReader.Create(reader);
            InitModel(xmlReader, model);
        }
        private static void InitModel(XmlReader reader, UIModel model) {
            var referencedAssets = new List<string>();
            var nodes = new List<UIGraphNode>();
            var xmlNodes = new List<UIGraphXmlNode>();
            var schema = UISchemaAsset.Schema;
            TypeDecomposer decomposer = new TypeDecomposer();
            if (reader.Read() && IsRoot(reader)) {
                if (reader.HasAttributes) {
                    if (reader.MoveToAttribute("output")) {
                        model.outputDirectory = reader.Value.Substring(0, reader.Value.LastIndexOf('/'));
                        model.outputFileName = reader.Value.Substring(reader.Value.LastIndexOf('/') + 1);
                    }
                    if (reader.MoveToAttribute("spriteTable")) {
                        model.spriteTable = AssetDatabase.LoadAssetAtPath<UISpriteTable>(reader.Value.StartsWith("guid:") ? AssetDatabase.GUIDToAssetPath(reader.Value.Substring(5)) : reader.Value);
                    }
                    reader.MoveToElement();
                }

                var childReader = reader.ReadSubtree();
                var root = UIGraphXmlNode.Create(null, default, -1);
                while (childReader.Read()) {
                    Parse(reader, root, -1, 0, schema, xmlNodes, decomposer);
                }
            }
            model.assets.AddRange(xmlNodes.SelectMany(xmlNode => xmlNode.assetReferences));
            model.nodes.AddRange(xmlNodes.Select(node => new UIModel.Node(node)));

        }
        private static bool IsRoot(XmlReader reader) => reader.NodeType == XmlNodeType.Element && reader.Name == ROOT_ELEMENT;
        public unsafe static void Parse(XmlReader reader, UIGraphXmlNode parent, int parentIndex, int depth, UISchemaAsset schema, List<UIGraphXmlNode> xmlNodes, TypeDecomposer decomposer) {
            while (reader.Read()) {
                if (reader.NodeType == XmlNodeType.Element && schema.Elements.TryGetValue(reader.Name, out UISchema.Element entry)) {
                    var node = UIGraphXmlNode.Create(reader.Name, entry, parentIndex);
                    if (reader.HasAttributes) {
                        var fields = decomposer.Decompose<UIConfig>('-');
                        var extraFields = decomposer.Decompose(node.element.config.Value, '-');
                        if (extraFields != null) {
                            foreach (var extraField in extraFields) {
                                if (!fields.ContainsKey(extraField.Key))
                                    fields[extraField.Key] = extraField.Value;
                            }
                        }
                        for (int i = 0; i < reader.AttributeCount; i++) {
                            reader.MoveToAttribute(i);
                            node.properties[reader.Name] = reader.Value;
                            if (fields.TryGetValue(reader.Name, out TypeDecomposer.FieldData data) && data.isAssetReference) {
                                node.assetReferences.Add(reader.Value);
                            }
                        }
                        reader.MoveToElement();
                    }
                    node.pass = node.element.pass.Value;
                    var index = xmlNodes.Count;
                    parent.children.Add(index);
                    xmlNodes.Add(node);

                    var subtree = reader.ReadSubtree();
                    subtree.Read();
                    Parse(subtree, node, index, depth + 1, schema, xmlNodes, decomposer);
                    subtree.Close();

                }
            }
        }



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
    }
}