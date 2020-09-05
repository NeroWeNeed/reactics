using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Reactics.Core.Commons;
using Reactics.Core.Commons.Reflection;
using Reactics.Core.Effects;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {


    public class ObjectGraphModel : ScriptableObject {
        public SortedDictionary<string, NodeEntry> entries = new SortedDictionary<string, NodeEntry>();

        public Dictionary<string, VariableEntry> variableEntries = new Dictionary<string, VariableEntry>();
        public HashSet<ObjectGraphVariable> variables = new HashSet<ObjectGraphVariable>();

        public object GetValue(string id, string key) {
            return entries[id].values[key];
        }
        public bool TryGetValue(string id, string key, out NodeEntry.Value value) {
            value = default;
            return entries.TryGetValue(id, out NodeEntry entry) && entry.values.TryGetValue(key, out value);
        }
        public object SetValue(string id, string key, object value) {
            return entries[id].values[key] = new NodeEntry.Value(value);
        }
        public object RemoveValue(string id, string key) {
            return entries[id].values.Remove(key);
        }
        public object HasValue(string id, string key) {
            return entries[id].values.ContainsKey(key);
        }
        public bool HasVariable(string id, string key) {
            return variableEntries.Values.Any((entry) => entry.targets.Any((target) => target.id == id && target.field == key));
        }
        public void SetEntryType(string id, Type type, object data) {
            var e = entries[id];
            if (e.type != type) {
                e.type = type;
                e.values.Clear();
                foreach (var fieldInfo in type.GetFields()) {
                    if (!fieldInfo.IsSerializableField())
                        continue;
                    var fieldValue = Activator.CreateInstance(fieldInfo.FieldType);
                    ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, data, out fieldValue);
                    e.values[fieldInfo.Name] = new NodeEntry.Value(fieldValue);
                }
                entries[id] = e;
            }
        }

        public void SetEntryType(string id, object source, object data) {
            var e = entries[id];
            if (e.type != source.GetType()) {
                e.type = source.GetType();
                e.values.Clear();
                foreach (var fieldInfo in source.GetType().GetFields()) {
                    if (!fieldInfo.IsSerializableField())
                        continue;
                    var fieldValue = fieldInfo.GetValue(source);
                    ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, data, out fieldValue);
                    e.values[fieldInfo.Name] = new NodeEntry.Value(fieldValue);
                }
                entries[id] = e;
            }
        }
        public void SetEntryNext(string id, string next) {
            var e = entries[id];
            e.next = next;
            entries[id] = e;
        }
        public string GetEntryNextId(string id) {
            return entries[id].next;
        }
        public NodeEntry GetEntryNext(string id) {
            return entries[entries[id].next];
        }
        public NodeEntry CreateEntry(Type type, string id = null, string next = null, Dictionary<string, NodeEntry.Value> values = null, bool overwrite = true) {
            if (!overwrite && id != null && entries.TryGetValue(id, out NodeEntry e)) {
                return e;
            }
            id = id == null ? Guid.NewGuid().ToString() : id;
            e = new NodeEntry(type, next, values);

            entries[id] = e;
            return e;
        }
        public NodeEntry CreateEntry(Type type, out string id, string next = null, Dictionary<string, NodeEntry.Value> values = null) {


            id = Guid.NewGuid().ToString();
            var e = new NodeEntry(type, next, values);
            entries[id] = e;
            return e;
        }
        public void SetEntry(string id, NodeEntry entry, bool overwrite = true) {
            if (!overwrite && id != null && entries.ContainsKey(id)) {
                return;
            }
            entries[id] = entry;
        }
        public void SetEntryData(string id, object source, object data, string next = null) {
            var entry = entries[id];
            entries[id] = new NodeEntry(source, next == null ? entry.next : next, data);
        }
        public bool RemoveEntry(string id) {
            var r = entries.Remove(id);
            if (r) {
                foreach (var key in entries.Keys) {
                    var entry = entries[key];
                    if (entry.next == id) {
                        entry.next = null;
                        entries[key] = entry;
                    }
                }
            }
            return r;
        }
        public NodeEntry GetEntry(string id) {
            return entries[id];
        }
        public KeyValuePair<string, NodeEntry.Value>[] GetEntryValues(string id) {
            var e = entries[id];
            return e.values.ToArray();

        }
        public bool TryGetEntry(string id, out NodeEntry entry) {
            return entries.TryGetValue(id, out entry);
        }
        public NodeEntry GetOrCreateEntry(string id, Type type, string next = null, Dictionary<string, NodeEntry.Value> values = null) {
            if (entries.TryGetValue(id, out NodeEntry entry)) {
                return entry;
            }
            else {
                return CreateEntry(type, id, next, values, true);
            }
        }
        public VariableEntry CreateVariableEntry(ObjectGraphVariable variable, string id = null, IEnumerable<VariableEntry.Target> targets = null, bool overwrite = true) {
            if (!overwrite && id != null && variableEntries.TryGetValue(id, out VariableEntry e)) {
                if (e.variable != variable)
                    throw new ArgumentException("Variable Entry already exists with different type");
                return e;
            }

            id = id == null ? Guid.NewGuid().ToString() : id;
            e = new VariableEntry(variable);
            if (targets != null)
                e.targets.AddRange(targets);
            variableEntries[id] = e;
            return e;

        }
        public bool RemoveVariableEntry(string id) {
            return variableEntries.Remove(id);
        }
        public bool TryGetVariableEntry(string id, out VariableEntry entry) {
            return variableEntries.TryGetValue(id, out entry);
        }
        public VariableEntry GetVariableEntry(string id) {
            return variableEntries[id];
        }
        public VariableEntry GetOrCreateVariableEntry(string id, ObjectGraphVariable variable, IEnumerable<VariableEntry.Target> targets = null) {
            if (variableEntries.TryGetValue(id, out VariableEntry entry)) {
                return entry;
            }
            else {
                return CreateVariableEntry(variable, id, targets, true);
            }
        }
        public void SetVariableEntryTargets(string id, IEnumerable<VariableEntry.Target> targets) {
            if (variableEntries.TryGetValue(id, out VariableEntry entry)) {
                entry.targets.Clear();
                if (targets != null)
                    entry.targets.AddRange(targets);
            }
        }
        public bool AddVariable(ObjectGraphVariable variable) {
            return this.variables.Add(variable);
        }
        public bool RemoveVariable(ObjectGraphVariable variable) {
            return this.variables.Remove(variable);
        }
        public ObjectGraphVariable GetVariable(Variable variable, string typeName) {
            return this.variables.FirstOrDefault((v) => v.MatchAddress(variable, typeName));
        }
        public ObjectGraphVariable GetVariable(string containerTypeName, string address) {
            return this.variables.FirstOrDefault((v) => v.containerType.AssemblyQualifiedName == containerTypeName && v.address == address);
        }
        public struct NodeEntry : IEquatable<NodeEntry> {
            public Type type;
            public string next;
            public Dictionary<string, Value> values;
            public NodeEntry(Type type = null, string next = null, object data = null) {
                this.type = type;
                this.next = next;
                this.values = new Dictionary<string, Value>();
                foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                    if (!fieldInfo.IsSerializableField())
                        continue;

                    var fieldValue = Activator.CreateInstance(fieldInfo.FieldType);
                    ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, data, out fieldValue);
                    this.values[fieldInfo.Name] = new Value(fieldValue.GetType(), fieldValue);

                }


            }
            public NodeEntry(NodeEntry other) {
                this.type = other.type;
                this.next = other.next;
                this.values = new Dictionary<string, Value>(other.values);
            }
            public NodeEntry(object source, string next, object data) {
                values = new Dictionary<string, Value>();
                foreach (var fieldInfo in source.GetType().GetFields()) {
                    if (!fieldInfo.IsSerializableField())
                        continue;

                    var fieldValue = source == null ? Activator.CreateInstance(fieldInfo.FieldType) : fieldInfo.GetValue(source);
                    ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, data, out fieldValue);
                    values[fieldInfo.Name] = new Value(fieldValue.GetType(), fieldValue);

                }
                type = source.GetType();
                this.next = next;

            }
            //public static void ConfigureEntryValues(Entry[] entries,objc)
            public void InitValues(object source) {
                if (type != source.GetType()) {
                    type = source.GetType();
                }

            }
            public override string ToString() {
                return $"Entry(Type: {type}, Next: {next}, Values: [{(values.Count > 0 ? values.Select((kv) => $"{kv.Key}: {kv.Value}").Aggregate((initial, next) => $"{initial}, {next}") : "")}]";
            }

            public override bool Equals(object obj) {
                return obj is NodeEntry entry &&
                       EqualityComparer<Type>.Default.Equals(type, entry.type) &&
                       next == entry.next &&
                       EqualityComparer<Dictionary<string, Value>>.Default.Equals(values, entry.values);
            }

            public bool Equals(NodeEntry other) {
                return EqualityComparer<Type>.Default.Equals(type, other.type) &&
                       next == other.next &&
                       EqualityComparer<Dictionary<string, Value>>.Default.Equals(values, other.values);
            }

            public override int GetHashCode() {
                int hashCode = 1061983682;
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(next);
                hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<string, Value>>.Default.GetHashCode(values);
                return hashCode;
            }
            public struct Value : IEquatable<Value> {
                public Type type;
                public object value;

                public Value(Type type, object value) {
                    this.type = type;

                    this.value = value;
                }
                public Value(object value) {
                    this.type = value.GetType();
                    this.value = value;
                }
                public Value(Type type) {
                    this.type = type;
                    this.value = Activator.CreateInstance(type);
                }

                public override bool Equals(object obj) {
                    return obj is Value value &&
                           EqualityComparer<Type>.Default.Equals(type, value.type) &&
                           EqualityComparer<object>.Default.Equals(this.value, value.value);
                }

                public bool Equals(Value other) {
                    return EqualityComparer<Type>.Default.Equals(type, other.type) &&
                            EqualityComparer<object>.Default.Equals(this.value, other.value);
                }

                public override int GetHashCode() {
                    int hashCode = -45697740;
                    hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
                    hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(value);
                    return hashCode;
                }
            }
        }
        public struct VariableEntry : IEquatable<VariableEntry> {

            public readonly ObjectGraphVariable variable;
            public readonly List<Target> targets;

            public VariableEntry(ObjectGraphVariable variable) {
                this.variable = variable;
                this.targets = new List<Target>();
            }

            public override bool Equals(object obj) {
                return obj is VariableEntry entry &&
                       EqualityComparer<ObjectGraphVariable>.Default.Equals(variable, entry.variable) &&
                       EqualityComparer<List<Target>>.Default.Equals(targets, entry.targets);
            }

            public bool Equals(VariableEntry other) {
                return EqualityComparer<ObjectGraphVariable>.Default.Equals(variable, other.variable) &&
                       EqualityComparer<List<Target>>.Default.Equals(targets, other.targets);
            }

            public override int GetHashCode() {
                int hashCode = 271935918;
                hashCode = hashCode * -1521134295 + EqualityComparer<ObjectGraphVariable>.Default.GetHashCode(variable);
                hashCode = hashCode * -1521134295 + EqualityComparer<List<Target>>.Default.GetHashCode(targets);
                return hashCode;
            }

            public struct Target {
                public string id;
                public string field;
            }
        }
    }

    public interface IObjectGraphModelEntry {

    }





}