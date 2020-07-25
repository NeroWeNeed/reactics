using System.Xml.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace Reactics.Commons
{
    [Serializable]
    public class EnumDictionary<TEnum, TValue> : IDictionary<TEnum, TValue>, ISerializationCallbackReceiver where TEnum : Enum
    {

        public TValue this[TEnum key]
        {
            get
            {
                var r = values[GetIndex(key)];
                if (r.exists)
                    return r.value;
                else
                    throw new KeyNotFoundException();
            }
            set
            {
                var i = GetIndex(key);
                if (values[i].exists)
                {
                    Count++;
                }
                values[i] = new Entry
                {
                    key = key,
                    value = value,
                    exists = true
                };
            }
        }

        private KeyCollection keys;
        public ICollection<TEnum> Keys
        {
            get
            {
                if (keys == null) keys = new KeyCollection(this);
                return keys;
            }
        }

        private ValueCollection valueCollection;
        public ICollection<TValue> Values
        {
            get
            {
                if (valueCollection == null) valueCollection = new ValueCollection(this);
                return valueCollection;
            }
        }
        public int Count { get; private set; }
        public bool IsReadOnly { get; private set; }
        [SerializeField]
        private Entry[] values;

        private uint length;

        private int version;

        public EnumDictionary()
        {
            Initialize();
        }
        public EnumDictionary(IDictionary<TEnum, TValue> dictionary)
        {
            Initialize();
            foreach (var kv in dictionary)
            {
                Add(kv.Key, kv.Value);
            }
        }
        public EnumDictionary(IEnumerable<KeyValuePair<TEnum, TValue>> collection)
        {
            Initialize();
            foreach (var kv in collection)
            {
                Add(kv.Key, kv.Value);
            }
        }

        private void Initialize()
        {
            var enums = Enum.GetValues(typeof(TEnum));
            values = new Entry[enums.Length];
            var isSet = new bool[enums.Length];
            length = (uint)enums.Length;
            for (int i = 0; i < enums.Length; i++)
            {

                uint index = Convert.ToUInt32((TEnum)enums.GetValue(i)) % length;
                while (isSet[index])
                {
                    index = (index + 1) % length;
                }
                values[index] = new Entry
                {
                    key = (TEnum)enums.GetValue(i),
                    value = default,
                    exists = false
                };
                isSet[index] = true;
            }

        }
        private int GetIndex(TEnum key)
        {
            uint index = Convert.ToUInt32(key) % length;
            var orig = index;
            while (!values[index].key.Equals(key))
            {
                index = (index + 1) % length;
                if (orig == index)
                    throw new ArgumentException("Invalid Enum Entry");
            }
            return (int)index;
        }
        public void Add(TEnum key, TValue value)
        {
            var index = GetIndex(key);
            if (!values[index].exists)
            {
                Count++;
            }
            values[index] = new Entry
            {
                key = key,
                value = value,
                exists = true
            };
            version++;

        }
        public void Add(KeyValuePair<TEnum, TValue> item)
        {
            var index = GetIndex(item.Key);
            if (!values[index].exists)
            {
                Count++;
            }
            values[index] = new Entry
            {
                key = item.Key,
                value = item.Value,
                exists = true
            };
            version++;
        }

        public void Clear()
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = new Entry
                {
                    key = values[i].key,
                    value = default,
                    exists = false
                };
            }
            Count = 0;
            version++;
        }

        public bool Contains(KeyValuePair<TEnum, TValue> item)
        {
            var index = GetIndex(item.Key);
            return values[index].exists && EqualityComparer<TValue>.Default.Equals(values[index].value, item.Value);
        }

        public bool ContainsKey(TEnum key)
        {
            var index = Array.IndexOf(Enum.GetValues(typeof(TEnum)), key);
            return values[index].exists;
        }

        public void CopyTo(KeyValuePair<TEnum, TValue>[] array, int arrayIndex)
        {
            if (array == null || arrayIndex + Count >= array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            int offset = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].exists)
                    array[arrayIndex + offset++] = new KeyValuePair<TEnum, TValue>(values[i].key, values[i].value);
            }
        }

        public IEnumerator<KeyValuePair<TEnum, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }
        public bool Remove(TEnum key)
        {
            var index = GetIndex(key);
            if (values[index].exists)
            {
                Count--;
                values[index] = new Entry
                {
                    key = values[index].key,
                    value = default,
                    exists = false
                };
                version++;
                return true;
            }
            return false;

        }

        public bool Remove(KeyValuePair<TEnum, TValue> item)
        {
            var index = GetIndex(item.Key);
            if (values[index].exists && EqualityComparer<TValue>.Default.Equals(item.Value, values[index].value))
            {
                Count--;
                values[index] = new Entry
                {
                    key = values[index].key,
                    value = default,
                    exists = false
                };
                version++;
                return true;
            }
            return false;
        }

        public bool TryGetValue(TEnum key, out TValue value)
        {
            var entry = values[GetIndex(key)];
            if (entry.exists)
            {
                value = entry.value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {

            return new Enumerator(this);
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            Count = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].exists)
                    Count++;
            }

        }

        [Serializable]
        public struct Entry
        {
            [SerializeField]
            internal TEnum key;
            public TEnum Key { get => key; }
            [SerializeField]
            internal TValue value;

            public TValue Value { get => value; }
            [SerializeField]
            internal bool exists;

        }
        public struct Enumerator : IEnumerator<KeyValuePair<TEnum, TValue>>, IDictionaryEnumerator
        {
            public KeyValuePair<TEnum, TValue> Current
            {
                get
                {
                    if (index == 0 || index == reference.values.Length + 1)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    return new KeyValuePair<TEnum, TValue>(current.Key, current.Value);
                }
            }

            private Entry current;
            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == reference.values.Length + 1)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    return current;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    if (index == 0 || index == reference.values.Length + 1)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    return new DictionaryEntry(current.Key, current.Value);
                }
            }

            public object Key
            {
                get
                {
                    if (index == 0 || index == reference.values.Length + 1)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    return current.Key;
                }
            }

            public object Value
            {
                get
                {
                    if (index == 0 || index == reference.values.Length + 1)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    return current.Value;
                }
            }
            private int index;

            private EnumDictionary<TEnum, TValue> reference;

            private int version;
            internal Enumerator(EnumDictionary<TEnum, TValue> dictionary)
            {
                version = dictionary.version;
                reference = dictionary;
                index = 0;
                current = default;

            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (version != reference.version)
                {
                    throw new InvalidOperationException("Dictionary has been changed.");
                }
                while (index < reference.values.Length)
                {

                    if (reference.values[index].exists)
                    {
                        current = new Entry
                        {
                            key = reference.values[index].key,
                            value = reference.values[index].value,
                            exists = true
                        };
                        index++;
                        return true;
                    }
                    index++;
                }
                current = default;
                index = reference.values.Length + 1;
                return false;
            }

            public void Reset()
            {
                if (index == 0 || index == reference.values.Length + 1)
                {
                    throw new InvalidOperationException("Dictionary has been changed.");
                }
                index = 0;
                current = default;
            }
        }


        public sealed class KeyCollection : ICollection<TEnum>, ICollection, IReadOnlyCollection<TEnum>
        {
            public int Count => reference.Count;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => ((ICollection)reference).SyncRoot;
            private EnumDictionary<TEnum, TValue> reference;
            public KeyCollection(EnumDictionary<TEnum, TValue> reference)
            {
                this.reference = reference;
            }
            public void Add(TEnum item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TEnum item)
            {
                return reference.ContainsKey(item);
            }

            public void CopyTo(TEnum[] array, int arrayIndex)
            {
                if (array == null || arrayIndex + Count >= array.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                int offset = 0;
                for (int i = 0; i < reference.values.Length; i++)
                {
                    if (reference.values[i].exists)
                        array[arrayIndex + offset++] = reference.values[i].key;
                }
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null || index + Count >= array.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                int offset = 0;
                for (int i = 0; i < reference.values.Length; i++)
                {
                    if (reference.values[i].exists)
                        array.SetValue(reference.values[i].key, index + offset++);
                }
            }

            public IEnumerator<TEnum> GetEnumerator()
            {
                return new Enumerator(reference);
            }

            public bool Remove(TEnum item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(reference);
            }
            public struct Enumerator : IEnumerator<TEnum>, IEnumerator
            {
                private EnumDictionary<TEnum, TValue> reference;
                private int version;
                private int index;

                private TEnum current;
                public object Current
                {
                    get
                    {
                        if (index == 0 || index == reference.values.Length + 1)
                        {
                            throw new InvalidOperationException("Dictionary has been changed.");
                        }
                        return current;
                    }
                }

                TEnum IEnumerator<TEnum>.Current
                {
                    get
                    {
                        if (index == 0 || index == reference.values.Length + 1)
                        {
                            throw new InvalidOperationException("Dictionary has been changed.");
                        }
                        return current;
                    }
                }

                public Enumerator(EnumDictionary<TEnum, TValue> reference)
                {
                    this.reference = reference;
                    this.version = reference.version;
                    index = 0;
                    current = default;

                }
                public void Dispose() { }

                public bool MoveNext()
                {
                    if (version != reference.version)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    while (index < reference.values.Length)
                    {

                        if (reference.values[index].exists)
                        {
                            current = reference.values[index].key;
                            index++;
                            return true;
                        }
                        index++;
                    }
                    current = default;
                    index = reference.values.Length + 1;
                    return false;
                }

                public void Reset()
                {
                    if (version != reference.version)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    index = 0;
                    current = default;
                }
            }
        }


        public sealed class ValueCollection : ICollection<TValue>, ICollection, IReadOnlyCollection<TValue>
        {

            public int Count => reference.Count;

            public bool IsReadOnly => true;

            public bool IsSynchronized => false;

            public object SyncRoot => ((ICollection)reference).SyncRoot;

            private EnumDictionary<TEnum, TValue> reference;
            public ValueCollection(EnumDictionary<TEnum, TValue> reference)
            {
                this.reference = reference;
            }
            public void Add(TValue item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TValue item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                if (array == null || arrayIndex + Count >= array.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                int offset = 0;
                for (int i = 0; i < reference.values.Length; i++)
                {
                    if (reference.values[i].exists)
                        array[arrayIndex + offset++] = reference.values[i].value;
                }
            }

            public void CopyTo(Array array, int index)
            {
                if (array == null || index + Count >= array.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }
                int offset = 0;
                for (int i = 0; i < reference.values.Length; i++)
                {
                    if (reference.values[i].exists)
                        array.SetValue(reference.values[i].value, index + offset++);
                }
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return new Enumerator(reference);
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(reference);
            }
            public struct Enumerator : IEnumerator<TValue>, IEnumerator
            {
                private EnumDictionary<TEnum, TValue> reference;
                private int version;
                private int index;

                private TValue current;
                public object Current
                {
                    get
                    {
                        if (index == 0 || index == reference.values.Length + 1)
                        {
                            throw new InvalidOperationException("Dictionary has been changed.");
                        }
                        return current;
                    }
                }
                TValue IEnumerator<TValue>.Current
                {
                    get
                    {
                        if (index == 0 || index == reference.values.Length + 1)
                        {
                            throw new InvalidOperationException("Dictionary has been changed.");
                        }
                        return current;
                    }
                }

                public Enumerator(EnumDictionary<TEnum, TValue> reference)
                {
                    this.reference = reference;
                    this.version = reference.version;
                    index = 0;
                    current = default;

                }
                public void Dispose() { }

                public bool MoveNext()
                {
                    if (version != reference.version)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    while (index < reference.values.Length)
                    {

                        if (reference.values[index].exists)
                        {
                            current = reference.values[index].value;
                            index++;
                            return true;
                        }
                        index++;
                    }
                    current = default;
                    index = reference.values.Length + 1;
                    return false;
                }

                public void Reset()
                {
                    if (version != reference.version)
                    {
                        throw new InvalidOperationException("Dictionary has been changed.");
                    }
                    index = 0;
                    current = default;
                }
            }
        }
    }



}