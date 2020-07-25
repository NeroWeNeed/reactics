using System;
using System.Collections.Generic;
using UnityEngine;
namespace Reactics.Commons
{

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<KeyValuePair<TKey, TValue>> serializedValues;
        public void OnAfterDeserialize()
        {
            this.Clear();
            if (serializedValues != null && serializedValues.Count > 0)
            {
                foreach (var item in serializedValues)
                {
                    this[item.Key] = item.Value;
                }
                serializedValues.Clear();

            }
        }

        public void OnBeforeSerialize()
        {
            if (serializedValues == null)
                serializedValues = new List<KeyValuePair<TKey, TValue>>();
            if (serializedValues.Count > 0)
            {
                foreach (var item in serializedValues)
                {
                    this[item.Key] = item.Value;
                }
                serializedValues.Clear();
            }
            foreach (var item in this)
            {
                serializedValues.Add(item);
            }
        }
        [Serializable]
        private struct Entry : IEquatable<Entry>
        {
            public TKey key;
            public TValue value;

            public override bool Equals(object obj)
            {
                return obj is Entry entry &&
                       EqualityComparer<TKey>.Default.Equals(key, entry.key) &&
                       EqualityComparer<TValue>.Default.Equals(value, entry.value);
            }

            public bool Equals(Entry other)
            {
                return EqualityComparer<TKey>.Default.Equals(key, other.key) &&
                       EqualityComparer<TValue>.Default.Equals(value, other.value);
            }

            public override int GetHashCode()
            {
                int hashCode = 1363396886;
                hashCode = hashCode * -1521134295 + EqualityComparer<TKey>.Default.GetHashCode(key);
                hashCode = hashCode * -1521134295 + EqualityComparer<TValue>.Default.GetHashCode(value);
                return hashCode;
            }
            public static explicit operator Entry(KeyValuePair<TKey, TValue> e) => new Entry { key = e.Key, value = e.Value };
        }
    }
}