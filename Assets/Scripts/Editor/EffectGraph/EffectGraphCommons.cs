using System.Reflection;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Reactics.Editor
{
    public class EffectGraphEntryConverter : JsonConverter<EffectGraph.Entry>
    {
        public override EffectGraph.Entry ReadJson(JsonReader reader, Type objectType, EffectGraph.Entry existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Dictionary<string, object> values;
            if (hasExistingValue)
            {
                existingValue.Values.Clear();
                values = existingValue.Values;
            }
            else
                values = new Dictionary<string, object>();
            if (reader.TokenType == JsonToken.StartObject)
            {
                var model = JObject.Load(reader);
                foreach (var item in JArray.Load(model["values"].CreateReader()))
                {
                    var key = (string)item["name"];
                    var value = serializer.Deserialize(item["value"].CreateReader(), Type.GetType((string)item["type"]));
                    values.Add(key, value);
                }
                EffectGraph.Entry result;
                if (hasExistingValue)
                {
                    result = existingValue;
                    result.Type = Type.GetType((string)model["type"]);
                }
                else
                {
                    result = new EffectGraph.Entry(Guid.Parse((string)model["id"]), values, Array.Empty<Guid>(), Type.GetType((string)model["type"]))
                    {
                        NextId = Guid.Parse((string)model["nextId"])
                    };
                }
                return result;
            }
            else
                return hasExistingValue ? existingValue : null;

        }

        public override void WriteJson(JsonWriter writer, EffectGraph.Entry value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(value.Type.AssemblyQualifiedName);
            writer.WritePropertyName("id");
            writer.WriteValue(value.Id);
            writer.WritePropertyName("nextId");
            writer.WriteValue(value.NextId);
            writer.WritePropertyName("values");
            writer.WriteStartArray();
            foreach (var entry in value.Values)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteValue(entry.Key);
                writer.WritePropertyName("type");
                writer.WriteValue(entry.Value.GetType().AssemblyQualifiedName);
                writer.WritePropertyName("value");
                serializer.Serialize(writer, entry.Value, entry.Value.GetType());
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    public class RectConverter : JsonConverter<Rect>
    {
        public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            if (hasExistingValue)
            {
                existingValue.xMin = obj.Value<float>("left");
                existingValue.xMax = obj.Value<float>("right");
                existingValue.yMin = obj.Value<float>("top");
                existingValue.yMax = obj.Value<float>("bottom");
                return existingValue;
            }
            else
            {
                return new Rect(obj.Value<float>("left"), obj.Value<float>("top"), obj.Value<float>("right") - obj.Value<float>("left"), obj.Value<float>("bottom") - obj.Value<float>("top"));
            }
        }

        public override void WriteJson(JsonWriter writer, Rect value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("left");
            writer.WriteValue(value.xMin);
            writer.WritePropertyName("top");
            writer.WriteValue(value.yMin);
            writer.WritePropertyName("right");
            writer.WriteValue(value.xMax);
            writer.WritePropertyName("bottom");
            writer.WriteValue(value.yMax);
            writer.WriteEndObject();

        }
    }

    public interface INodeReader
    {
        Node[] Collect(Node node);
        bool IsRoot(Node node);
        Type GetNodeType(Node node);

    }

    [Serializable]
    public struct NodeIndex : IEquatable<NodeIndex>
    {
        public Type type;

        public Guid node;
        public NodeIndex(Type portType)
        {
            this.type = portType;
            node = Guid.Empty;
        }

        public NodeIndex(Type portType, Guid node) : this(portType)
        {
            this.node = node;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeIndex index &&
                   EqualityComparer<Type>.Default.Equals(type, index.type) &&
                   EqualityComparer<Guid>.Default.Equals(node, index.node);
        }

        public bool Equals(NodeIndex other)
        {
            return EqualityComparer<Type>.Default.Equals(type, other.type) &&
                   EqualityComparer<Guid>.Default.Equals(node, other.node);
        }

        public override int GetHashCode()
        {
            int hashCode = -1114374285;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(node);
            return hashCode;
        }

        public override string ToString()
        {
            return "NodeIndex(" + type.FullName + ", " + node + ")";
        }
    }

    public sealed class OutputContainerElement : Attribute { }
}