using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactics.Core.Commons;
using UnityEditor;
using UnityEngine;

namespace Reactics.Editor.Graph {

    public class JsonObjectGraphSerializer : ObjectGraphSerializer<JsonObjectGraphCollection> {
        private static readonly JsonSerializerSettings Settings;
        static JsonObjectGraphSerializer() {
            Settings = JsonConvert.DefaultSettings();
            Settings.TypeNameHandling = TypeNameHandling.All;
        }
        public override bool Deserialize(JsonObjectGraphCollection target, IObjectGraphNodeProvider module, ObjectGraphView graphView, out JsonObjectGraphCollection result) {
            if (string.IsNullOrEmpty(target.json)) {
                result = target;
                return false;
            }
            var jsonSet = new ObjectGraphNodeJsonSet(graphView.MasterNode.viewDataKey, JsonConvert.DeserializeObject<ObjectGraphNodeJsonSet>(target.json, Settings));
            var nodes = new Dictionary<ObjectGraphNode, ObjectGraphNodeJsonSet.Entry>();
            bool initiated = false;
            Vector2 topLeft = Vector2.zero;
            foreach (var jsonEntry in jsonSet.entries) {
                foreach (var m in graphView.Modules.OfType<IObjectGraphNodeProvider>()) {
                    var node = m.Create(jsonEntry);
                    if (node != null) {
                        if (initiated) {
                            topLeft.x = topLeft.x > jsonEntry.layout.position.x ? jsonEntry.layout.position.x : topLeft.x;
                            topLeft.y = topLeft.y > jsonEntry.layout.position.y ? jsonEntry.layout.position.y : topLeft.y;
                        }
                        else {
                            topLeft = jsonEntry.layout.position;
                            initiated = true;
                        }
                        nodes[node] = jsonEntry;
                        graphView.Model.SetEntry(node.Id, jsonEntry.entry);
                        graphView.AddElement(node);
                        break;
                    }
                }
            }

            foreach (var kv in nodes) {
                kv.Key.Refresh();
                kv.Key.SetPosition(kv.Value.layout.Offset(graphView.LastMousePosition - topLeft));
            }
            graphView.Validate();
            result = new JsonObjectGraphCollection
            {
                json = target.json,
                nodes = nodes.Keys.ToArray()
            };
            return true;
        }

        public override bool Serialize(JsonObjectGraphCollection target, IObjectGraphNodeProvider module, ObjectGraphView graphView, out JsonObjectGraphCollection result) {
            if (target.nodes?.Length > 0) {
                result = new JsonObjectGraphCollection
                {
                    nodes = target.nodes,
                    json = JsonConvert.SerializeObject(new ObjectGraphNodeJsonSet(graphView.MasterNode.viewDataKey, target.nodes.Select((t) => Tuple.Create(t, graphView.Model.entries[t.Id])).ToArray()), Settings)
                };
                return true;

            }
            else {
                result = target;
                return false;
            }
        }
    }
    public struct JsonObjectGraphCollection {
        public ObjectGraphNode[] nodes;

        public string json;
    }
    public struct ObjectGraphNodeJsonSet {

        public Entry[] entries;

        public ObjectGraphNodeJsonSet(string master, params Tuple<ObjectGraphNode, ObjectGraphModel.NodeEntry>[] nodes) {
            var entries = new Entry[nodes.Length];
            string[] newIds = new string[nodes.Length];
            for (int i = 0; i < nodes.Length; i++) {
                entries[i] = new Entry(nodes[i].Item1, nodes[i].Item2);
                newIds[i] = Guid.NewGuid().ToString();
            }
            ConfigureEntries(entries, newIds, master);
            this.entries = entries;
        }
        public ObjectGraphNodeJsonSet(string master, ObjectGraphNodeJsonSet source) {
            var entries = new Entry[source.entries.Length];
            string[] newIds = new string[source.entries.Length];
            for (int i = 0; i < source.entries.Length; i++) {
                entries[i] = new Entry(source.entries[i]);
                newIds[i] = Guid.NewGuid().ToString();
            }
            ConfigureEntries(entries, newIds, master);
            this.entries = entries;
        }
        private static void ConfigureEntries(Entry[] entries, string[] newIds, string master) {
            for (int i = 0; i < entries.Length; i++) {
                if (!string.IsNullOrEmpty(entries[i].entry.next)) {
                    var newNext = Array.FindIndex(entries, (e) => e.id == entries[i].entry.next);
                    if (newNext >= 0)
                        entries[i].entry.next = newIds[newNext];
                    else
                        entries[i].entry.next = null;
                }
                var keys = entries[i].entry.values.Keys.ToArray();
                foreach (var key in keys) {
                    var entryValue = entries[i].entry.values[key];
                    if (entryValue.value is IObjectGraphNodeValueCopyCallback callback) {
                        entryValue.value = callback.OnCopy(entries, newIds, master);
                        entries[i].entry.values[key] = entryValue;
                    }
                }

            }
            for (int i = 0; i < entries.Length; i++) {
                entries[i].id = newIds[i];
            }
        }
        public struct Entry {

            public Type nodeType;
            public ObjectGraphModel.NodeEntry entry;

            public string id;

            public Rect layout;

            public Entry(ObjectGraphNode node, ObjectGraphModel.NodeEntry entry) {

                this.entry = entry;
                id = node.viewDataKey;
                layout = node.GetPosition();
                nodeType = node.GetType();
            }
            public Entry(Entry original) {
                this.entry = new ObjectGraphModel.NodeEntry(original.entry);
                id = original.id;
                layout = original.layout;
                nodeType = original.nodeType;
            }

        }
    }

    public class ObjectGraphModelEntryJsonConverter : JsonConverter<ObjectGraphModel.NodeEntry> {
        public override ObjectGraphModel.NodeEntry ReadJson(JsonReader reader, Type objectType, ObjectGraphModel.NodeEntry existingValue, bool hasExistingValue, JsonSerializer serializer) {
            Dictionary<string, object> values = new Dictionary<string, object>();
            if (reader.TokenType == JsonToken.StartObject) {
                var model = JObject.Load(reader);
                foreach (var item in JArray.Load(model["values"].CreateReader())) {
                    var key = (string)item["key"];
                    var value = serializer.Deserialize(item["value"].CreateReader(), Type.GetType((string)item["type"]));
                    values.Add(key, value);
                }
                return new ObjectGraphModel.NodeEntry(Type.GetType((string)model["type"]), (string)model["next"], values);
            }
            else {
                return default;
            }
        }

        public override void WriteJson(JsonWriter writer, ObjectGraphModel.NodeEntry value, JsonSerializer serializer) {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            serializer.Serialize(writer, value.type);
            writer.WritePropertyName("next");
            serializer.Serialize(writer, value.next);
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var kv in value.values) {
                writer.WriteStartObject();
                writer.WritePropertyName("key");
                serializer.Serialize(writer, kv.Key);
                writer.WritePropertyName("value");
                serializer.Serialize(writer, kv.Value);
                writer.WritePropertyName("type");
                serializer.Serialize(writer, kv.Value.GetType());
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();

        }
    }
    public interface IObjectGraphNodeValueCopyCallback {
        object OnCopy(ObjectGraphNodeJsonSet.Entry[] entries, string[] newIds, string master);
    }
}