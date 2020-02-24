using System;
using System.Collections;
using System.Collections.Generic;

namespace Reactics.Util
{

    public class GuidStore<TValue> : IEnumerable<TValue>
    {
        public const int INITIAL_SIZE = 16;

        public const float LOADING_FACTOR = 0.75f;
        private ValueEntry[] valueEntries;

        private KeyEntry[] keyEntries;

        public int Count { get; private set; }

        public int PublicKeyCount { get; private set; }

        public GuidStore()
        {
            valueEntries = new ValueEntry[INITIAL_SIZE];
            keyEntries = new KeyEntry[INITIAL_SIZE];
            
        }
        public PrivateKey Add(TValue value)
        {
            if (Count > valueEntries.Length * LOADING_FACTOR)
            {
                valueEntries = Rehash(valueEntries);
            }
            return Add(new ValueEntry(value));
        }
        public PrivateKey Add(TValue value, PublicKey publicKey)
        {
            if (Count > valueEntries.Length * LOADING_FACTOR)
            {
                valueEntries = Rehash(valueEntries);
            }
            PrivateKey privateKey = Add(new ValueEntry(value));
            SetPublicKey(publicKey, privateKey);
            return privateKey;
        }
        private PrivateKey Add(TValue value, PrivateKey key)
        {
            if (Count > valueEntries.Length * LOADING_FACTOR)
            {
                valueEntries = Rehash(valueEntries);
            }
            return Add(new ValueEntry(value, key));
        }
        private PrivateKey Add(ValueEntry entry)
        {
            AddToHashArray(valueEntries, entry);
            Count++;
            return entry.PrivateKey;
        }
        private void AddToHashArray<T>(T[] array, T element) where T : class, ILinked<T>
        {
            int index = MathUtils.FloorMod(element.GetHashCode(), array.Length);
            if (array[index] == null)
            {
                array[index] = element;
            }
            else
            {
                T temp = array[index];
                while (temp.GetNext() != null)
                    temp = temp.GetNext();
                temp.SetNext(element);
            }

        }
        public PublicKey CreatePublicKey(PrivateKey key)
        {
            if (PublicKeyCount > keyEntries.Length * LOADING_FACTOR)
            {
                keyEntries = Rehash(keyEntries);
            }
            KeyEntry entry = new KeyEntry(key);
            AddToHashArray(keyEntries, entry);
            PublicKeyCount++;
            return entry.PublicKey;
        }
        private void SetPublicKey(PublicKey publicKey, PrivateKey key)
        {
            if (PublicKeyCount > keyEntries.Length * LOADING_FACTOR)
            {
                keyEntries = Rehash(keyEntries);
            }
            KeyEntry entry = new KeyEntry(publicKey, key);
            if (!TryGet(publicKey, out TValue value))
            {
                AddToHashArray(keyEntries, entry);
                PublicKeyCount++;

            }
            else
            {
                throw new ArgumentException("Public Key is already in use");
            }


        }

        public void RemovePublicKeys(PrivateKey key)
        {
            KeyEntry entry;
            KeyEntry temp;
            for (int i = 0; i < keyEntries.Length; i++)
            {
                entry = keyEntries[i];
                if (entry == null)
                    continue;
                if (entry.PrivateKey.Equals(key))
                {
                    keyEntries[i] = entry.GetNext();
                    PublicKeyCount--;
                    temp = entry.GetNext();
                    entry.SetNext(null);
                    entry = temp;
                }
                while (entry.GetNext() != null)
                {
                    if (entry.GetNext().PrivateKey.Equals(key))
                    {
                        temp = entry.GetNext();
                        entry.SetNext(null);
                        PublicKeyCount--;
                        entry = temp.GetNext();
                        temp.SetNext(null);
                    }
                    else
                    {
                        entry = entry.GetNext();
                    }

                }
            }
        }

        public bool Remove(PrivateKey key)
        {
            int index = MathUtils.FloorMod(key.GetHashCode(), valueEntries.Length);
            if (valueEntries[index] == null)
            {
                return false;
            }
            else
            {
                ValueEntry temp = valueEntries[index];

                if (temp.PrivateKey.Equals(key))
                {
                    valueEntries[index] = temp.GetNext();
                    temp.SetNext(null);
                    Count--;
                    RemovePublicKeys(key);
                    return true;
                }
                else
                {
                    while (temp.GetNext() != null)
                    {
                        if (temp.GetNext().PrivateKey.Equals(key))
                        {
                            ValueEntry temp2 = temp.GetNext();
                            temp.SetNext(temp.GetNext().GetNext());
                            temp2.SetNext(null);
                            Count--;
                            RemovePublicKeys(key);
                            return true;
                        }
                        temp = temp.GetNext();
                    }

                    return false;
                }

            }
        }

        public TValue this[PrivateKey key]
        {
            get
            {
                int index = MathUtils.FloorMod(key.GetHashCode(), valueEntries.Length);
                ValueEntry temp = valueEntries[index];
                if (temp == null)
                {
                    throw new ArgumentException("No Value is present for provided key.");
                }
                else if (temp.PrivateKey.Equals(key))
                    return temp.Value;
                else
                {
                    while (temp.GetNext() != null)
                    {
                        if (temp.GetNext().PrivateKey.Equals(key))
                        {
                            return temp.GetNext().Value;
                        }
                        temp = temp.GetNext();
                    }
                    throw new ArgumentException("No Value is present for provided key.");
                }
            }
            set
            {
                int index = MathUtils.FloorMod(key.GetHashCode(), valueEntries.Length);
                ValueEntry temp = valueEntries[index];
                if (temp == null)
                {
                    Add(value, key);
                    return;
                }
                else if (temp.PrivateKey.Equals(key))
                {
                    temp.Value = value;
                    return;
                }
                else
                {
                    while (temp.GetNext() != null)
                    {
                        if (temp.GetNext().PrivateKey.Equals(key))
                        {
                            temp.Value = value;
                            return;
                        }
                        temp = temp.GetNext();
                    }
                    Add(value, key);

                }

            }
        }

        public TValue this[PublicKey key]
        {
            get
            {
                int index = MathUtils.FloorMod(key.GetHashCode(), keyEntries.Length);
                KeyEntry temp = keyEntries[index];
                if (temp == null)
                {
                    throw new ArgumentException("No Value is present for provided key.");
                }
                else if (temp.PublicKey.Equals(key))
                    return this[temp.PrivateKey];
                else
                {
                    while (temp.GetNext() != null)
                    {
                        if (temp.GetNext().PublicKey.Equals(key))
                        {
                            return this[temp.GetNext().PrivateKey];
                        }
                        temp = temp.GetNext();
                    }
                    throw new ArgumentException("No Value is present for provided key.");
                }
            }
        }

        private bool TryGet(PublicKey key, out TValue value)
        {

            int index = MathUtils.FloorMod(key.GetHashCode(), keyEntries.Length);
            KeyEntry temp = keyEntries[index];
            if (temp == null)
            {
                value = default;
                return false;
            }
            else if (temp.PublicKey.Equals(key))
            {
                value = this[temp.PrivateKey];
                return true;
            }

            else
            {
                while (temp.GetNext() != null)
                {
                    if (temp.GetNext().PublicKey.Equals(key))
                    {
                        value = this[temp.GetNext().PrivateKey];
                        return true;
                    }
                    temp = temp.GetNext();
                }
                value = default;
                return false;
            }
        }
        public bool Contains(PrivateKey key)
        {

            int index = MathUtils.FloorMod(key.GetHashCode(), keyEntries.Length);
            ValueEntry temp = valueEntries[index];
            if (temp == null)
            {
                return false;
            }
            else if (temp.PrivateKey.Equals(key))
            {
                return true;
            }

            else
            {
                while (temp.GetNext() != null)
                {
                    if (temp.GetNext().PrivateKey.Equals(key))
                    {
                        return true;
                    }
                    temp = temp.GetNext();
                }
                return false;
            }
        }
        private T[] Rehash<T>(T[] array) where T : class, ILinked<T>
        {
            T[] newArray = new T[array.Length * 2];
            T element;
            T temp;
            for (int i = 0; i < array.Length; i++)
            {
                element = array[i];
                while (element != null)
                {
                    int index = MathUtils.FloorMod(element.GetHashCode(), newArray.Length);
                    if (newArray[index] == null)
                    {
                        newArray[index] = element;
                    }
                    else
                    {
                        temp = newArray[index];
                        while (temp.GetNext() != null)
                            temp = temp.GetNext();
                        temp.SetNext(element);

                    }
                    temp = element.GetNext();
                    element.SetNext(null);
                    element = temp;
                }
            }
            return newArray;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            ValueEntry entry;
            for (int i = 0; i < valueEntries.Length; i++)
            {
                entry = valueEntries[i];
                while (entry != null) {
                    yield return entry.Value;
                    entry = entry.GetNext();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private interface ILinked<TLinked>
        {
            void SetNext(TLinked next);

            TLinked GetNext();

        }

        private class ValueEntry : ILinked<ValueEntry>
        {
            public TValue Value;

            public PrivateKey PrivateKey;

            private ValueEntry next = null;

            public ValueEntry(TValue value, PrivateKey privateKey)
            {
                Value = value;
                PrivateKey = privateKey;
            }
            public ValueEntry(TValue value)
            {
                Value = value;
                PrivateKey = PrivateKey.Create();
            }

            public override int GetHashCode()
            {
                return PrivateKey.GetHashCode();
            }

            public void SetNext(ValueEntry next)
            {
                this.next = next;
            }

            public ValueEntry GetNext()
            {
                return next;
            }
        }
        private class KeyEntry : ILinked<KeyEntry>
        {

            public PublicKey PublicKey;

            public PrivateKey PrivateKey;

            public KeyEntry next = null;


            public KeyEntry(PublicKey publicKey, PrivateKey privateKey)
            {
                PublicKey = publicKey;
                PrivateKey = privateKey;
            }
            public KeyEntry(PrivateKey privateKey)
            {
                PublicKey = PublicKey.Create();
                PrivateKey = privateKey;
            }


            public override int GetHashCode()
            {
                return PublicKey.GetHashCode();
            }

            public void SetNext(KeyEntry next)
            {
                this.next = next;
            }

            public KeyEntry GetNext()
            {
                return next;
            }
        }
    }
    [Serializable]
    public struct PrivateKey
    {
        public static PrivateKey Create()
        {
            return new PrivateKey(Guid.NewGuid());
        }
        public static PrivateKey Create(Guid value)
        {
            return new PrivateKey(value);
        }
        private Guid Value;
        private PrivateKey(Guid value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            return obj is PrivateKey key &&
                   Value.Equals(key.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }
    }
    [Serializable]
    public struct PublicKey
    {
        public static PublicKey Create()
        {
            return new PublicKey(Guid.NewGuid());
        }
        public static PublicKey Create(Guid value)
        {
            return new PublicKey(value);
        }
        private Guid Value;
        private PublicKey(Guid value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            return obj is PublicKey key &&
                   Value.Equals(key.Value);
        }

        public override int GetHashCode()
        {
            return -1937169414 + Value.GetHashCode();
        }
    }

}