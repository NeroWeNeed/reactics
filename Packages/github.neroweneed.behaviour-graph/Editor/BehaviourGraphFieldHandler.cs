using System;
using System.Collections.Generic;
using System.Reflection;
using NeroWeNeed.BehaviourGraph.Editor.Model;
using NeroWeNeed.Commons;
using NeroWeNeed.Commons.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: SearchableAssembly]
namespace NeroWeNeed.BehaviourGraph.Editor {

    public static class BehaviourGraphFieldHandlers {
        private static readonly Dictionary<Type, BehaviourGraphFieldHandler> handlers = new Dictionary<Type, BehaviourGraphFieldHandler>();
        [InitializeOnLoadMethod]
        private static void CollectHandlers() {
            handlers.Clear();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.GetCustomAttribute<SearchableAssemblyAttribute>() != null) {
                    foreach (var type in assembly.GetTypes()) {

                        var attr = type.GetCustomAttribute<BehaviourGraphFieldHandlerAttribute>();
                        if (attr == null)
                            continue;
                        if (!type.IsSubclassOf(typeof(BehaviourGraphFieldHandler))) {
                            Debug.LogError($"'{type}' does not inherit '{nameof(BehaviourGraphFieldHandler)}'");
                            continue;
                        }
                        if (handlers.ContainsKey(attr.Type)) {
                            Debug.LogError($"Multiple Handlers registered for type '{attr.Type}'");
                            continue;
                        }
                        if (type.GetConstructor(Array.Empty<Type>()) == null) {
                            Debug.LogError($"No default constructor found for Handler '{type}'");
                            continue;
                        }
                        handlers[attr.Type] = Activator.CreateInstance(type) as BehaviourGraphFieldHandler;
                    }
                }
            }
        }
        public static bool TryGetBehaviourGraphFieldHandler(Type type, out BehaviourGraphFieldHandler handler) {
            return handlers.TryGetValue(type, out handler);
        }
        public static BehaviourGraphFieldHandler GetBehaviourGraphFieldHandler(Type type) {
            return handlers[type];
        }

    }
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BehaviourGraphFieldHandlerAttribute : Attribute {
        public Type Type { get; }
        public BehaviourGraphFieldHandlerAttribute(Type type) {
            Type = type;
        }
    }
    public abstract class BehaviourGraphFieldHandler {

        public abstract HandleData Initialize(Node node, BehaviourEntry entry, BehaviourGraphView graphView, BehaviourGraphSettings settings, int index);
        public abstract string Serialize(Node node, BehaviourGraphView graphView, BehaviourGraphSettings settings, FieldOffsetInfo fieldOffsetInfo, VisualElement element);

        public struct HandleData {
            public VisualElement element;

            public NodeTarget target;
        }
        public enum NodeTarget : byte {
            Input, Output, Extension
        }
    }

}