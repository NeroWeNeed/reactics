using System;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace NeroWeNeed.Commons {
    [Serializable]
    public struct SerializableMethod {
        public SerializableType container;
        public string name;
        public bool IsCreated { get => container.IsCreated && !string.IsNullOrEmpty(name); }
        [JsonIgnore]
        public MethodInfo Value { get => container.Value?.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static); }
        public SerializableMethod(MethodInfo method) {
            container = new SerializableType(method?.DeclaringType);
            name = method?.Name;
        }
        public static implicit operator SerializableMethod(MethodInfo method) => new SerializableMethod(method);
    }


}