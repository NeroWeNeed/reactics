using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public static class ObjectGraphModuleUtility {
        public static bool TryGetTargetType(Type targetType, Type inspectionType, Type[] types, out Type result) {
            if (inspectionType == null) {
                result = null;
                return false;
            }
            else if (inspectionType.IsGenericType && inspectionType.GetGenericTypeDefinition().Equals(targetType)) {
                result = inspectionType;
                return true;
            }
            else {
                result = Array.Find(inspectionType.GetInterfaces(), (i) => i.IsGenericType && i.GetGenericTypeDefinition().Equals(targetType) && Array.IndexOf(types, i) != -1);
                return result != null;
            }
        }
        public static Type GetTargetType(Type targetType, Type inspectionType, Type[] types) {
            if (TryGetTargetType(targetType, inspectionType, types, out Type result)) {
                return result;
            }
            else {
                throw new ArgumentException("Unknown Type", nameof(targetType));
            }
        }
        public static Type[] GetTargetTypes(Type targetType, Type inspectionType, Type[] types) {
            if (inspectionType == null) {
                return null;
            }
            if (inspectionType.IsGenericType && inspectionType.GetGenericTypeDefinition().Equals(targetType)) {
                return new Type[] { inspectionType };
            }
            else {
                return inspectionType.GetInterfaces().Where((i) => i.IsGenericType && i.GetGenericTypeDefinition().Equals(targetType) && Array.IndexOf(types, i) != -1).ToArray();
            }
        }


        public static List<SearchTreeEntry> CreateSearchEntries(string name, int depth, IObjectGraphNodeProvider provider, IEnumerable<Type> rootTypes, IEnumerable<Type> validTypes) {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent(name),depth)
            };
            var typeGroups = new Dictionary<Type, List<Type>>();
            foreach (var type in rootTypes) {
                typeGroups[type] = new List<Type>();
            }
            foreach (var type in validTypes) {
                foreach (var key in typeGroups.Keys) {
                    if (key.IsAssignableFrom(type)) {
                        typeGroups[key].Add(type);
                        break;
                    }
                }
            }
            foreach (var kv in typeGroups) {
                tree.Add(new SearchTreeGroupEntry(new GUIContent($"{kv.Key.GenericTypeArguments[0].Name} {name}"), depth + 1));
                foreach (var type in kv.Value) {
                    var content = new GUIContent(type.Name);
                    tree.Add(new SearchTreeEntry(content)
                    {
                        level = depth + 2,
                        userData = new SearchTreeEntryData
                        {
                            provider = provider,
                            type = type
                        }
                    });
                }
            }
            return tree;
        }

    }
}