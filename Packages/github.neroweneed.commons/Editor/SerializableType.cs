using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace NeroWeNeed.Commons.Editor {
    [Serializable]
    public struct SerializableType : IEquatable<SerializableType>, IEquatable<Type> {

        [SerializeField]
        internal string assemblyQualifiedName;

        [NonSerialized]
        private Type type;
        public Type Value => type ??= assemblyQualifiedName == null ? null : Type.GetType(assemblyQualifiedName);

        public SerializableType(Type type) {
            assemblyQualifiedName = type?.AssemblyQualifiedName;
            this.type = type;
        }
        public static implicit operator SerializableType(Type type) => new SerializableType(type);

        public static bool operator ==(SerializableType self, Type type) {
            return self.Equals(type);
        }

        public static bool operator !=(SerializableType self, Type type) {
            return !self.Equals(type);
        }
        public static bool operator ==(SerializableType self, SerializableType type) {
            return self.Equals(type);
        }

        public static bool operator !=(SerializableType self, SerializableType type) {
            return !self.Equals(type);
        }

        public bool Equals(Type other) {
            return this.assemblyQualifiedName == other.AssemblyQualifiedName;
        }

        public bool Equals(SerializableType other) {
            return this.assemblyQualifiedName == other.assemblyQualifiedName;
        }

        public override bool Equals(object obj) {
            if (obj is SerializableType serializableType) {
                return Equals(serializableType);
            }
            else if (obj is Type type) {
                return Equals(type);
            }
            else {
                return false;
            }
        }

        public override int GetHashCode() {
            int hashCode = 35819425;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(assemblyQualifiedName);
            return hashCode;
        }

        public override string ToString() {
            return assemblyQualifiedName;
        }
    }

    public class SerializableTypeConverter : JsonConverter<SerializableType> {

        public override SerializableType ReadJson(JsonReader reader, Type objectType, SerializableType existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var name = reader.Value as string;
            return new SerializableType(name == null ? null : Type.GetType(name));
        }

        public override void WriteJson(JsonWriter writer, SerializableType value, JsonSerializer serializer) {
            writer.WriteValue(value.assemblyQualifiedName);
        }
    }

}