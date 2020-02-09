using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Reactics.Util
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ResourceField : Attribute
    {

        public readonly string path;

        public ResourceField(string path)
        {
            this.path = path;
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DelegateField : Attribute
    {
        public readonly string methodName;

        public DelegateField(string methodName = "")
        {
            this.methodName = methodName;
        }
    }


    public static class UnityObjectUtil
    {
        public static void InjectResources(this UnityEngine.Object target)
        {
            ResourceField attr;

            foreach (var item in target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                attr = item.GetCustomAttribute(typeof(ResourceField)) as ResourceField;
                if (attr == null || item.GetValue(target) != default)
                    continue;
                if (Application.isEditor)
                    item.SetValue(target, AssetDatabase.LoadAssetAtPath($"Assets/Resources/{attr.path}", item.FieldType));
                else
                    item.SetValue(target, Resources.Load(attr.path, item.FieldType));
            }
        }
        public static void InjectDelegates<T>(this T target) where T : UnityEngine.Object => target.InjectDelegates(target.GetType());
        public static void InjectDelegates<T,S>(this T target,S source) where T : UnityEngine.Object => target.InjectDelegates(target.GetType(),source,source.GetType());
        public static void InjectDelegates(this UnityEngine.Object target, Type targetType,object source,Type sourceType)
        {
            DelegateField attr;
            Dictionary<string, FieldInfo> delegateFields = new Dictionary<string, FieldInfo>();
            string fieldName;
            foreach (var item in targetType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                attr = item.GetCustomAttribute(typeof(DelegateField)) as DelegateField;
                if (attr == null || item.GetValue(target) != default || !typeof(Delegate).IsAssignableFrom(item.FieldType))
                    continue;
                if (attr.methodName == string.Empty)
                {
                    fieldName = item.Name;
                    if (fieldName == string.Empty)
                        continue;
                    fieldName = char.ToUpper(fieldName[0]).ToString() + (fieldName.Length > 1 ? fieldName.Substring(1) : "");
                }
                else
                {
                    fieldName = attr.methodName;
                }
                delegateFields[fieldName] = item;
            }
            foreach (var item in sourceType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                
                if (delegateFields.ContainsKey(item.Name))
                {
                    
                    delegateFields[item.Name].SetValue(target, item.CreateDelegate(delegateFields[item.Name].FieldType, source));
                }
            }
        }

    }
}