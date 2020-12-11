using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;


namespace NeroWeNeed.Commons {
    [Serializable]
    public class SerializableReferenceDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue> {
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeReference]
        private List<TValue> values = new List<TValue>();

        public int Count { get => dictionary.Count; }


        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public Dictionary<TKey, TValue> Value { get => dictionary; }

        public ICollection<TKey> Keys => throw new NotImplementedException();

        public ICollection<TValue> Values => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return dictionary.GetEnumerator();
        }

        public void OnAfterDeserialize() {
            dictionary.Clear();
            for (int i = 0; i < keys.Count; i++) {
                dictionary[keys[i]] = values[i];
            }
        }

        public void OnBeforeSerialize() {
            keys.Clear();
            values.Clear();
            foreach (var kv in dictionary) {
                keys.Add(kv.Key);
                values.Add(kv.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return dictionary.GetEnumerator();
        }
        public void Add(TKey key, TValue value) {
            dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key) {
            return dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key) {
            return dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            dictionary.Add(item.Key, item.Value);
        }

        public void Clear() {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {

            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            return dictionary.Remove(item.Key);
        }
    }
    /*     
        public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver {
            [SerializeField]
            private int count;
            [SerializeField]
            private int allocatedSize;
            [SerializeField]
            private float loadingFactor;

            [SerializeField]
            private Bucket[] buckets;
            private int version;

            public TValue this[TKey key]
            {
                get
                {

                    var index = MathCommons.FloorMod(key.GetHashCode(), allocatedSize);
                    foreach (var node in buckets[index].nodes) {
                        if (EqualityComparer<TKey>.Default.Equals(key, node.key)) {
                            return node.value;
                        }
                    }
                    throw new KeyNotFoundException();
                }
                set
                {
                    Add(key, value);
                }
            }

            private KeyCollection keys;
            public ICollection<TKey> Keys
            {
                get
                {
                    return keys ??= new KeyCollection(this);
                }
            }
            private ValueCollection values;
            public ICollection<TValue> Values
            {
                get
                {
                    return values ??= new ValueCollection(this);
                }
            }

            public int Count { get => count; private set => count = value; }

            //TODO
            public bool IsReadOnly => false;



            public SerializableDictionary(int initialSize = 8, float loadingFactor = 0.75f) {
                Contract.Requires(initialSize > 0);
                Contract.Requires(loadingFactor > 0 && loadingFactor <= 1);
                this.allocatedSize = PowerOf2(initialSize);
                this.buckets = new Bucket[allocatedSize];

            }
            private void ExpandAndRehash() {
                var newAllocatedSize = PowerOf2(allocatedSize + 1);
                var newBuckets = new Bucket[newAllocatedSize];
                foreach (var bucket in buckets) {
                    if (!bucket.IsCreated)
                        continue;
                    foreach (var node in bucket.nodes) {
                        int index = MathCommons.FloorMod(node.key.GetHashCode(), allocatedSize);
                        newBuckets[index].Add(node, index);
                    }
                }
                this.allocatedSize = newAllocatedSize;
                this.buckets = newBuckets;


            }
            private void EnsureCapacity() {
                if (count < allocatedSize * loadingFactor) {
                    return;
                }
                else {
                    ExpandAndRehash();
                }

            }
            private int PowerOf2(int value) => (int)Math.Pow(2, Math.Ceiling(Math.Log(value) / Math.Log(2)));
            private void ValidateBucketLayout() {
                var newBuckets = new Bucket[allocatedSize];
                for (int i = 0; i < buckets.Length; i++) {
                    if (!buckets[i].IsCreated)
                        continue;
                    newBuckets[buckets[i].index] = new Bucket
                    {
                        nodes = buckets[i].nodes,
                        index = buckets[i].index
                    };
                }
                this.buckets = newBuckets;
            }
            public void Add(TKey key, TValue value) {
                version++;
                EnsureCapacity();
                var index = MathCommons.FloorMod(key.GetHashCode(), allocatedSize);
                Debug.Log(index);
                if (buckets[index].Add(new Node
                {
                    key = key,
                    value = value
                }, index)) {
                    Count++;
                }
            }

            public void Add(KeyValuePair<TKey, TValue> item) {
                version++;
                EnsureCapacity();
                var index = MathCommons.FloorMod(item.Key.GetHashCode(), allocatedSize);
                if (buckets[index].Add(new Node
                {
                    key = item.Key,
                    value = item.Value
                }, index)) {
                    Count++;
                }
            }

            public void Clear() {
                if (Count > 0) {
                    Count = 0;
                    for (int i = 0; i < buckets.Length; i++) {
                        buckets[i] = default;
                    }
                    version++;
                }
            }

            public bool Contains(KeyValuePair<TKey, TValue> item) {
                var index = MathCommons.FloorMod(item.Key.GetHashCode(), allocatedSize);
                foreach (var node in buckets[index].nodes) {
                    if (EqualityComparer<TKey>.Default.Equals(item.Key, node.key) && EqualityComparer<TValue>.Default.Equals(item.Value, node.value)) {
                        return true;
                    }
                }
                return false;
            }

            public bool ContainsKey(TKey key) {
                var index = MathCommons.FloorMod(key.GetHashCode(), allocatedSize);
                foreach (var node in buckets[index].nodes) {
                    if (EqualityComparer<TKey>.Default.Equals(key, node.key)) {
                        return true;
                    }
                }
                return false;
            }
            public bool ContainsValue(TValue value) {
                foreach (var bucket in buckets) {
                    if (!bucket.IsCreated)
                        continue;
                    foreach (var node in bucket.nodes) {
                        if (EqualityComparer<TValue>.Default.Equals(value, node.value)) {
                            return true;
                        }
                    }
                }

                return false;
            }
            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
                if (array == null)
                    throw new ArgumentNullException("array");
                if (arrayIndex < 0 || arrayIndex >= array.Length)
                    throw new ArgumentOutOfRangeException("arrayIndex");
                if (array.Length - arrayIndex < Count)
                    throw new ArgumentException("Target Array too small");

                for (int bucketIndex = 0; bucketIndex < buckets.Length; bucketIndex++) {
                    if (!buckets[bucketIndex].IsCreated)
                        continue;
                    foreach (var node in buckets[bucketIndex].nodes) {
                        array[arrayIndex++] = new KeyValuePair<TKey, TValue>(node.key, node.value);
                    }
                }
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
                return new Enumerator(this);
            }

            public bool Remove(TKey key) {
                var index = MathCommons.FloorMod(key.GetHashCode(), allocatedSize);
                if (buckets[index].IsCreated) {
                    for (int i = 0; i < buckets[index].nodes.Count; i++) {
                        if (EqualityComparer<TKey>.Default.Equals(key, buckets[index].nodes[i].key)) {
                            buckets[index].nodes.RemoveAt(i);
                            version++;
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool Remove(KeyValuePair<TKey, TValue> item) {
                var index = MathCommons.FloorMod(item.Key.GetHashCode(), allocatedSize);
                if (buckets[index].IsCreated) {
                    for (int i = 0; i < buckets[index].nodes.Count; i++) {
                        if (EqualityComparer<TKey>.Default.Equals(item.Key, buckets[index].nodes[i].key) && EqualityComparer<TValue>.Default.Equals(item.Value, buckets[index].nodes[i].value)) {
                            buckets[index].nodes.RemoveAt(i);
                            version++;
                            return true;
                        }
                    }
                }
                return false;
            }

            public bool TryGetValue(TKey key, out TValue value) {
                var index = MathCommons.FloorMod(key.GetHashCode(), allocatedSize);
                foreach (var node in buckets[index].nodes) {
                    if (EqualityComparer<TKey>.Default.Equals(key, node.key)) {
                        value = node.value;
                        return true;
                    }
                }
                value = default;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return new Enumerator(this);
            }

            public void OnBeforeSerialize() {
            }

            public void OnAfterDeserialize() {
                ValidateBucketLayout();
            }

            [Serializable]
            public struct Bucket {
                public bool IsCreated { get => nodes != null; }
                public List<Node> nodes;
                public int index;


                public bool Add(Node node, int bucketIndex) {
                    this.index = bucketIndex;
                    if (nodes == null) {
                        nodes = new List<Node>
                        {
                            node
                        };
                        return true;
                    }
                    else {
                        for (int i = 0; i < nodes.Count; i++) {
                            if (EqualityComparer<TKey>.Default.Equals(nodes[i].key, node.key)) {
                                nodes[i] = new Node
                                {
                                    key = node.key,
                                    value = node.value
                                };
                                return false;
                            }
                        }
                        nodes.Add(new Node
                        {
                            key = node.key,
                            value = node.value
                        });
                        return true;
                    }


                }
            }
            [Serializable]
            public struct Node {
                [SerializeField]
                public TKey key;
                [SerializeReference]
                public TValue value;
                public static implicit operator KeyValuePair<TKey, TValue>(Node node) => new KeyValuePair<TKey, TValue>(node.key, node.value);
            }

            public sealed class KeyCollection : ICollection<TKey> {
                private readonly SerializableDictionary<TKey, TValue> dictionary;
                public int Count => dictionary.Count;

                public bool IsReadOnly => true;

                public KeyCollection(SerializableDictionary<TKey, TValue> dictionary) {
                    this.dictionary = dictionary;
                }

                public void Add(TKey item) {
                    throw new NotSupportedException();
                }

                public void Clear() {
                    throw new NotSupportedException();
                }

                public bool Contains(TKey item) {
                    return dictionary.ContainsKey(item);
                }

                public void CopyTo(TKey[] array, int arrayIndex) {
                    if (array == null)
                        throw new ArgumentNullException(nameof(array));
                    if (arrayIndex < 0 || arrayIndex >= array.Length)
                        throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                    if (array.Length - arrayIndex < Count)
                        throw new ArgumentException("Target Array too small");

                    for (int bucketIndex = 0; bucketIndex < dictionary.buckets.Length; bucketIndex++) {
                        if (!dictionary.buckets[bucketIndex].IsCreated)
                            continue;
                        foreach (var node in dictionary.buckets[bucketIndex].nodes) {
                            array[arrayIndex++] = node.key;
                        }
                    }
                }

                public IEnumerator<TKey> GetEnumerator() {
                    return new Enumerator(dictionary);
                }

                public bool Remove(TKey item) {
                    throw new NotSupportedException();
                }

                IEnumerator IEnumerable.GetEnumerator() {
                    return new Enumerator(dictionary);
                }
                public struct Enumerator : IEnumerator<TKey> {
                    private SerializableDictionary<TKey, TValue> dictionary;
                    private int version;
                    private int majorIndex;
                    private int minorIndex;
                    public TKey Current { get => dictionary.buckets[majorIndex].nodes[minorIndex].key; }

                    object IEnumerator.Current => this.Current;

                    public Enumerator(SerializableDictionary<TKey, TValue> dictionary) {
                        this.dictionary = dictionary;
                        this.version = dictionary.version;
                        this.majorIndex = 0;
                        this.minorIndex = -1;

                    }

                    public void Dispose() {

                    }

                    public bool MoveNext() {
                        if (version != dictionary.version) {
                            throw new InvalidOperationException("Dictionary has been modified.");
                        }
                        while (majorIndex < dictionary.buckets.Length) {
                            if (!dictionary.buckets[majorIndex].IsCreated) {
                                majorIndex++;
                                minorIndex = -1;
                                continue;
                            }
                            if (dictionary.buckets[majorIndex].nodes.Count >= minorIndex + 1) {
                                majorIndex++;
                                minorIndex = -1;
                                continue;
                            }
                            minorIndex++;
                            return true;
                        }
                        return false;
                    }

                    public void Reset() {
                        majorIndex = 0;
                        minorIndex = -1;
                    }
                }
            }


            public sealed class ValueCollection : ICollection<TValue> {
                private readonly SerializableDictionary<TKey, TValue> dictionary;
                public int Count => dictionary.Count;

                public bool IsReadOnly => true;

                public ValueCollection(SerializableDictionary<TKey, TValue> dictionary) {
                    this.dictionary = dictionary;
                }

                public void Add(TValue item) {
                    throw new NotSupportedException();
                }

                public void Clear() {
                    throw new NotSupportedException();
                }

                public bool Contains(TValue item) {
                    return dictionary.ContainsValue(item);
                }

                public void CopyTo(TValue[] array, int arrayIndex) {
                    if (array == null)
                        throw new ArgumentNullException(nameof(array));
                    if (arrayIndex < 0 || arrayIndex >= array.Length)
                        throw new ArgumentOutOfRangeException(nameof(arrayIndex));
                    if (array.Length - arrayIndex < Count)
                        throw new ArgumentException("Target Array too small");

                    for (int bucketIndex = 0; bucketIndex < dictionary.buckets.Length; bucketIndex++) {
                        if (!dictionary.buckets[bucketIndex].IsCreated)
                            continue;
                        foreach (var node in dictionary.buckets[bucketIndex].nodes) {
                            array[arrayIndex++] = node.value;
                        }
                    }
                }

                public IEnumerator<TValue> GetEnumerator() {
                    return new Enumerator(dictionary);
                }

                public bool Remove(TValue item) {
                    throw new NotSupportedException();
                }

                IEnumerator IEnumerable.GetEnumerator() {
                    return new Enumerator(dictionary);
                }
                public struct Enumerator : IEnumerator<TValue> {
                    private SerializableDictionary<TKey, TValue> dictionary;
                    private int version;
                    private int majorIndex;
                    private int minorIndex;
                    public TValue Current { get => dictionary.buckets[majorIndex].nodes[minorIndex].value; }

                    object IEnumerator.Current => this.Current;

                    public Enumerator(SerializableDictionary<TKey, TValue> dictionary) {
                        this.dictionary = dictionary;
                        this.version = dictionary.version;
                        this.majorIndex = 0;
                        this.minorIndex = -1;

                    }

                    public void Dispose() {

                    }

                    public bool MoveNext() {
                        if (version != dictionary.version) {
                            throw new InvalidOperationException("Dictionary has been modified.");
                        }
                        while (majorIndex < dictionary.buckets.Length) {
                            if (!dictionary.buckets[majorIndex].IsCreated) {
                                majorIndex++;
                                minorIndex = -1;
                                continue;
                            }
                            if (dictionary.buckets[majorIndex].nodes.Count >= minorIndex + 1) {
                                majorIndex++;
                                minorIndex = -1;
                                continue;
                            }
                            minorIndex++;
                            return true;
                        }
                        return false;
                    }

                    public void Reset() {
                        majorIndex = 0;
                        minorIndex = -1;
                    }
                }
            }

            public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
                private SerializableDictionary<TKey, TValue> dictionary;
                private int version;
                private int majorIndex;
                private int minorIndex;
                public KeyValuePair<TKey, TValue> Current { get; private set; }

                object IEnumerator.Current => this.Current;

                public Enumerator(SerializableDictionary<TKey, TValue> dictionary) {
                    this.dictionary = dictionary;
                    this.version = dictionary.version;
                    this.majorIndex = 0;
                    this.minorIndex = -1;
                    this.Current = default;
                }

                public void Dispose() {

                }

                public bool MoveNext() {
                    if (version != dictionary.version) {
                        throw new InvalidOperationException("Dictionary has been modified.");
                    }
                    while (majorIndex < dictionary.buckets.Length) {
                        if (!dictionary.buckets[majorIndex].IsCreated) {
                            majorIndex++;
                            minorIndex = -1;
                            continue;
                        }
                        if (dictionary.buckets[majorIndex].nodes.Count >= minorIndex + 1) {
                            majorIndex++;
                            minorIndex = -1;
                            continue;
                        }
                        minorIndex++;
                        this.Current = dictionary.buckets[majorIndex].nodes[minorIndex];
                        return true;
                    }
                    return false;
                }

                public void Reset() {
                    majorIndex = 0;
                    minorIndex = -1;
                    this.Current = default;
                }
            }

        }
     */
}
