using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Reactics.Core.Commons;
using Reactics.Core.Commons.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor.Graph {


    public class ObjectGraphModel : ScriptableObject {
        public SortedDictionary<string, Entry> entries = new SortedDictionary<string, Entry>();


        public struct Entry : IEquatable<Entry> {
            public Type type;
            public string next;
            public Dictionary<string, object> values;

            public Entry(Type type, string next) {
                this.type = type;
                this.next = next;
                this.values = new Dictionary<string, object>();
            }
            public Entry(Type type, string next, Dictionary<string, object> values) {
                this.type = type;
                this.next = next;
                this.values = values;
            }
            public Entry(Entry other) {
                this.type = other.type;
                this.next = other.next;
                this.values = new Dictionary<string, object>(other.values);
            }
            public static Entry Create(object source, string next, object data) {
                var values = new Dictionary<string, object>();
                foreach (var fieldInfo in source.GetType().GetFields()) {
                    if (!fieldInfo.IsSerializableField())
                        continue;

                    var fieldValue = source == null ? Activator.CreateInstance(fieldInfo.FieldType) : fieldInfo.GetValue(source);
                    ObjectGraphNodeValueConverters.TryToConvertToAlias(fieldValue, data, out fieldValue);


                    values[fieldInfo.Name] = fieldValue;
                }

                return new Entry
                {
                    type = source.GetType(),
                    next = next,
                    values = values
                };
            }
            //public static void ConfigureEntryValues(Entry[] entries,objc)
            public void InitValues(object source) {
                if (type != source.GetType()) {
                    type = source.GetType();
                }

            }
            public override string ToString() {
                return $"Entry(Type: {type}, Next: {next}, Values: [{(values != null && values.Count > 0 ? values.Select((kv) => $"{kv.Key}: {kv.Value}").Aggregate((initial, next) => $"{initial}, {next}") : "")}]";
            }

            public override bool Equals(object obj) {
                return obj is Entry entry &&
                       EqualityComparer<Type>.Default.Equals(type, entry.type) &&
                       next == entry.next &&
                       EqualityComparer<Dictionary<string, object>>.Default.Equals(values, entry.values);
            }

            public bool Equals(Entry other) {
                return EqualityComparer<Type>.Default.Equals(type, other.type) &&
                       next == other.next &&
                       EqualityComparer<Dictionary<string, object>>.Default.Equals(values, other.values);
            }
        }
    }

}