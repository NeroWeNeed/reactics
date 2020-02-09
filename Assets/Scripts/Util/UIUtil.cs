using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Reactics.Battle;
using UnityEngine.SceneManagement;
using UnityEditor.UIElements;
using Reactics.Debugger;

namespace Reactics.Util
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UIElement : Attribute
    {
        public readonly string name;

        public UIElement(string name = "")
        {
            this.name = name;
        }
    }
    public static class UIUtil
    {
        private static Regex nameRegex = new Regex("\\-(.)");

        public static void Initialize(this VisualElement element, object target)
        {
            Initialize(element, target, target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Select(x => Tuple.Create(x.GetCustomAttribute<UIElement>(), x)).Where(x => x.Item1 != null).ToDictionary(x => nameRegex.Replace(x.Item1.name == string.Empty ? x.Item2.Name : x.Item1.name, (match) => match.Groups[1].Value.ToUpper()), x => x.Item2));
        }
        private static void Initialize(this VisualElement element, object target, Dictionary<string, FieldInfo> fields)
        {

            string methodName;
            string name;

            foreach (var item in element.Children())
            {
                if (item.name == string.Empty)
                    continue;
                name = nameRegex.Replace(item.name, (match) => match.Groups[1].Value.ToUpper());

                methodName = "Init" + name.Let((x) =>
                {
                    return char.ToUpper(x[0]) + x.Substring(1);
                });
                if (fields.TryGetValue(name, out FieldInfo info) && info.FieldType.IsAssignableFrom(item.GetType()))
                    info.SetValue(target, item);
                target.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).FirstOrDefault((method) => method.Name == methodName && method.GetParameters().Length == 1 && typeof(VisualElement).IsAssignableFrom(method.GetParameters()[0].ParameterType))?.Invoke(target, new object[] { item });
                if (item.childCount > 0)
                    Initialize(item, target, fields);
            }
        }
    }

};