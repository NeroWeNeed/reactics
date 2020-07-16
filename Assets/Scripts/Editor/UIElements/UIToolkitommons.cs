using System.Linq;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace Reactics.Editor {
    public static class UIToolkitUtility {
        public static void ConfigureToolbarButtons(this EditorWindow editorWindow, Toolbar toolbar) {
            ConfigureToolbarButtons(source: editorWindow, toolbar);
        }
        public static void ConfigureToolbarButtons(this UnityEditor.Editor editor, Toolbar toolbar) {
            ConfigureToolbarButtons(source: editor, toolbar);
        }
        private static void ConfigureToolbarButtons(object source, Toolbar toolbar) {
            Dictionary<string, Action> actions = new Dictionary<string, Action>();
            foreach (var method in source.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                if (method.IsAbstract || method.IsGenericMethod || method.GetParameters().Length > 0)
                    continue;
                var attr = method.GetCustomAttributes().OfType<ToolbarActionAttribute>().FirstOrDefault();
                if (attr == null)
                    continue;
                actions[attr.name] = (Action)method.CreateDelegate(typeof(Action), source);
            }

            toolbar.Query<ToolbarButton>().ForEach((button) =>
            {
                if (actions.TryGetValue(button.name, out Action action)) {
                    button.clicked += action;
                }
            });
        }
    }

    public sealed class ToolbarActionAttribute : Attribute {
        public string name;

        public ToolbarActionAttribute(string name) {
            this.name = name;
        }
    }
}