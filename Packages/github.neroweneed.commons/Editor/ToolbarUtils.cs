using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {
    public static class ToolbarUtility {
        public static Toolbar CreateFlatToolbar(object container) {
            var toolbar = new Toolbar();
            ToolbarItem attr;
            var items = new List<Entry>();
            foreach (var method in container.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                attr = method.GetCustomAttribute<ToolbarItem>();
                if (attr == null || method.IsGenericMethod || method.IsAbstract || method.GetParameters().Length > 0)
                    continue;
                items.Add(new Entry { attribute = attr, info = method });
            }
            foreach (var field in container.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                attr = field.GetCustomAttribute<ToolbarItem>();
                if (attr == null || field.FieldType != typeof(bool))
                    continue;
                items.Add(new Entry { attribute = attr, info = field });
            }
            items.Sort();
            for (int i = 0; i < items.Count; i++) {
                var item = items[i];
                toolbar.Add(item.GenerateTerminal(container));
                if (i + 1 < items.Count) {
                    toolbar.Add(new ToolbarSpacer());
                }
            }

            return toolbar;

        }
        /*         public static Toolbar CreateToolbar(object container) {
                    var toolbar = new Toolbar();

                    ToolbarItem attr;
                    var menus = new Dictionary<string, VisualElement>
                    {
                        [""] = toolbar
                    };
                    var items = new List<Entry>();
                    foreach (var method in container.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                        attr = method.GetCustomAttribute<ToolbarItem>();
                        if (attr == null || method.IsGenericMethod || method.IsAbstract || method.GetParameters().Length > 0)
                            continue;
                        items.Add(new Entry { attribute = attr, info = method });
                    }
                    foreach (var field in container.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                        attr = field.GetCustomAttribute<ToolbarItem>();
                        if (attr == null || field.FieldType != typeof(bool))
                            continue;
                        items.Add(new Entry { attribute = attr, info = field });
                    }
                    foreach (var item in items) {
                        AddEntry(menus, item, container);
                    }
                    return toolbar;

                } */
        /*         private static void AddEntry(Dictionary<string, VisualElement> menus, Entry entry, object target) {
                    var nodes = entry.attribute.Name.Split('/');
                    string currentPath = "";

                    for (int i = 0; i < nodes.Length; i++) {
                        if (string.IsNullOrEmpty(nodes[i]))
                            continue;
                        if (i + 1 < nodes.Length) {
                            VisualElement lastElement = menus[currentPath];
                            currentPath = NextMenu(currentPath, nodes[i]);

                            if (!menus.ContainsKey(currentPath)) {
                                var menu = new ToolbarMenu();
                                if (lastElement is Toolbar) {
                                    lastElement.Add(menu);
                                }
                                else if (lastElement is ToolbarMenu toolbarMenu) {
                                    toolbarMenu.menu.App
                                    lastElement.Add(menu);
                                }
                                menus[currentPath] = menu;
                            }
                        }
                        else {
                            VisualElement lastElement = menus[currentPath];
                            var terminal = entry.GenerateTerminal(nodes[i], target);
                            if (terminal != null) {
                                lastElement.Add(terminal);
                            }
                        }
                    }
                } */
        private static string NextMenu(string prefix, string next) {
            if (string.IsNullOrEmpty(prefix))
                return next;
            else
                return $"{prefix}/{next}";
        }
        private struct Entry : IComparable<Entry> {
            public ToolbarItem attribute;
            public MemberInfo info;

            public int CompareTo(Entry other) {
                return attribute.Order.CompareTo(other.attribute.Order);
            }
            public VisualElement GenerateTerminal(object target) {
                if (info is MethodInfo methodInfo) {
                    return new ToolbarButton(methodInfo.CreateDelegate(typeof(Action), target) as Action)
                    {
                        name = attribute.Name,
                        text = attribute.DisplayName,
                    };
                }
                else if (info is FieldInfo fieldInfo) {
                    var toggle = new ToolbarToggle
                    {
                        name = attribute.Name,
                        text = attribute.DisplayName,
                        userData = Tuple.Create(target, fieldInfo)
                    };
                    toggle.RegisterValueChangedCallback((evt) =>
                    {
                        var info = (evt.target as VisualElement).userData as Tuple<object, FieldInfo>;
                        info.Item2.SetValue(info.Item1, evt.newValue);
                    });
                    return toggle;
                }
                else {
                    return null;
                }
            }
        }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
    public sealed class ToolbarItem : Attribute {

        public string Name { get; private set; }
        public string DisplayName { get; private set; }

        public int Order { get; private set; }

        public ToolbarItem(string name, int order = -1) {
            Name = name;
            DisplayName = name;
            Order = order;
        }
        public ToolbarItem(string name, string displayName, int order = -1) {
            Name = name;
            DisplayName = displayName;
            Order = order;
        }
    }
}