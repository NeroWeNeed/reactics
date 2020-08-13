using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Reactics.Core.Commons.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {
    public class ObjectGraphModelEditor {

        public ObjectGraphModel Model { get; set; }
        public ObjectGraphModelEditor(ObjectGraphModel model = null) {
            if (model == null)
                this.Model = ScriptableObject.CreateInstance<ObjectGraphModel>();
            else
                this.Model = model;
        }


        public virtual void WriteEntryValue(ObjectGraphNode node, string key, object value) {
            var model = Model;
            if (model.entries.TryGetValue(node.viewDataKey, out ObjectGraphModel.Entry entry)) {
                entry.values[key] = value;
                model.entries[node.viewDataKey] = entry;
            }
        }


        public virtual void WriteNext(ObjectGraphNode node, string next) {
            var model = Model;
            if (model.entries.TryGetValue(node.viewDataKey, out ObjectGraphModel.Entry entry)) {
                entry.next = next;
                model.entries[node.viewDataKey] = entry;
            }

        }
        public virtual void SetEntry(ObjectGraphNode node, ObjectGraphModel.Entry entry) {
            var model = Model;
            if (Model != null) {
                model.entries[node.viewDataKey] = entry;
            }
        }
        public virtual void SetEntryFromObject(ObjectGraphNode node, object source) {
            var model = Model;
            var outputPort = node.OutputPort;
            var entry = ObjectGraphModel.Entry.Create(source, outputPort.connected ? outputPort.connections.First().input.node.viewDataKey : null, node);
            model.entries[node.viewDataKey] = entry;
        }


        public virtual ObjectGraphModel.Entry GetEntry(ObjectGraphNode node) => Model.entries[node.viewDataKey];

        public virtual bool TryGetEntry(ObjectGraphNode node, out ObjectGraphModel.Entry entry) {
            var model = Model;
            if (model != null) {
                var r = model.entries.TryGetValue(node.viewDataKey, out entry);
                return r;

            }
            else {
                entry = default;
                return false;
            }


        }

        public virtual void InitIfMissing(ObjectGraphNode node, object source = null) {
            foreach (var e in Model.entries) {
                Debug.Log(e);
            }
            if (!TryGetEntry(node, out ObjectGraphModel.Entry entry)) {
                InitEntry(node, source);
            }
        }
        public virtual ObjectGraphModel.Entry InitEntry(ObjectGraphNode node, object source = null) {
            var model = Model;

            var nextPort = node.Q<Port>(null, ObjectGraphNode.OutputPortClassName);
            string next = null;
            if (nextPort != null && nextPort.connected) {
                next = nextPort.connections.First()?.input?.node?.viewDataKey;
            }
            var values = new Dictionary<string, object>();
            foreach (var fieldInfo in node.TargetType.GetFields()) {
                if (!fieldInfo.IsSerializableField())
                    continue;

                var fieldType = fieldInfo.FieldType;
                var fieldValue = source == null ? Activator.CreateInstance(fieldInfo.FieldType) : fieldInfo.GetValue(source);

                ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, null, out fieldValue);
                fieldType = fieldValue.GetType();


                values[fieldInfo.Name] = fieldValue;
            }
            var entry = new ObjectGraphModel.Entry
            {
                type = node.TargetType,
                next = next,
                values = values
            };
            model.entries[node.viewDataKey] = entry;
            return entry;
        }

        public virtual void DeleteEntry(ObjectGraphNode node) {
            var model = Model;
            var guid = node.viewDataKey;
            model.entries.Remove(guid);
        }

    }

}