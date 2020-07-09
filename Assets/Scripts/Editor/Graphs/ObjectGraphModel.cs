using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reactics.Commons;
using Reactics.Commons.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph
{


    public class ObjectGraphModel : ScriptableObject
    {
        public SortedDictionary<string, Entry> entries = new SortedDictionary<string, Entry>();
        public struct Entry
        {
            public Type type;
            public string next;
            public Dictionary<string, object> values;
        }
    }
    public abstract class BaseObjectGraphModelEditor
    {
        public abstract ObjectGraphModel GetModel(ObjectGraphNode node);
        public virtual void WriteEntryValue(ObjectGraphNode node, string key, object value)
        {
            var model = GetModel(node);
            var entry = model.entries[node.viewDataKey];
            entry.values[key] = value;
            model.entries[node.viewDataKey] = entry;
        }

        public virtual void WriteNext(ObjectGraphNode node, string next)
        {
            var model = GetModel(node);
            var entry = model.entries[node.viewDataKey];
            entry.next = next;
            model.entries[node.viewDataKey] = entry;
        }

        public virtual ObjectGraphModel.Entry GetEntry(ObjectGraphNode node) => GetModel(node).entries[node.viewDataKey];

        public virtual void InitEntry(ObjectGraphNode node, object source = null)
        {
            var model = GetModel(node);
            var nextPort = node.Q<Port>(null, ObjectGraphNode.OutputPortClassName);
            string next = null;
            if (nextPort != null && nextPort.connected)
            {
                next = nextPort.connections.First()?.input?.node?.viewDataKey;
            }
            var values = new Dictionary<string, object>();
            foreach (var fieldInfo in node.TargetType.GetFields())
            {
                if (!fieldInfo.IsSerializableField())
                    continue;

                var fieldType = fieldInfo.FieldType;
                var fieldValue = source == null ? Activator.CreateInstance(fieldInfo.FieldType) : fieldInfo.GetValue(source);

                var aliasHandler = fieldInfo.FieldType.GetCustomAttribute<AliasHandler>();
                if (aliasHandler != null)
                {
                    fieldValue = AliasHandlers.ToAlias(fieldValue, aliasHandler, null);
                    fieldType = fieldValue.GetType();
                }

                values[fieldInfo.Name] = fieldValue;
            }
            model.entries[node.viewDataKey] = new ObjectGraphModel.Entry
            {
                type = node.TargetType,
                next = next,
                values = values
            };
        }

        public virtual void DeleteEntry(ObjectGraphNode node)
        {
            var model = GetModel(node);
            var guid = node.viewDataKey;
            model.entries.Remove(guid);
        }
    }
}