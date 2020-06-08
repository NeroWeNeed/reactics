using System.Xml.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Reactics.Commons
{


    [Serializable]

    public class EnumDictionary<TEnum, TValue> : ISerializationCallbackReceiver where TEnum : Enum
    {


        [SerializeField]
        private List<Entry> entries = new List<Entry>(Enum.GetValues(typeof(TEnum)).Length);
        private Dictionary<TEnum, TValue> dictionary = new Dictionary<TEnum, TValue>();

        public EnumDictionary(params TValue[] initialValues)
        {
            if (initialValues.Length > 0)
            {
                var enumValues = Enum.GetValues(typeof(TEnum));

                for (int i = 0; i < enumValues.Length; i++)
                {
                    if (i >= initialValues.Length)
                        dictionary[(TEnum)enumValues.GetValue(i)] = default;
                    else
                        dictionary[(TEnum)enumValues.GetValue(i)] = initialValues[i];
                }
            }

        }
        public EnumDictionary() {
            
        }

        public TValue this[TEnum key]
        {
            get
            {
                TValue result;
#if UNITY_EDITOR
                dictionary.TryGetValue(key, out result);
#else
                result = dictionary[key];
#endif
                return result;
            }
            set => dictionary[key] = value;
        }
        /*  public void OnAfterDeserialize()
        {

            if (dictionary != null)
                dictionary.Clear();
            else
                dictionary = new Dictionary<TEnum, TValue>();
            var validKeys = (TEnum[])Enum.GetValues(typeof(TEnum));
            foreach (var item in entries)
            {

                if (EqualityComparer<TValue>.Default.Equals(item.value, default))
                    dictionary[(TEnum) Enum.ToObject(typeof(TEnum),item.key)] = default;
                else
                    dictionary[(TEnum) Enum.ToObject(typeof(TEnum),item.key)] = (TValue)item.value;
            }
            foreach (var item in validKeys)
            {
                if (!dictionary.ContainsKey(item))
                    dictionary.Add(item, default);
            }

            entries.Clear();
        } */

        /*public void OnBeforeSerialize()
        {
            if (entries.Count > 0)
            {
                foreach (var entry in entries)
                {
                    dictionary[(TEnum) Enum.ToObject(typeof(TEnum),entry.key)] = entry.value;
                }
            }
            entries.Clear();
            var validKeys = (TEnum[])Enum.GetValues(typeof(TEnum));
            foreach (var item in validKeys)
            {
                var key = Convert.ToInt32(item);
                if (dictionary.TryGetValue(item, out TValue value))
                {
                    entries.Add(new Entry(key, value));
                }
                else
                    entries.Add(new Entry(key, default));

            }
            dictionary.Clear();
            entries.Sort();

        } */
        public void OnAfterDeserialize()
        {
            var keys = Enum.GetValues(typeof(TEnum));
            if (dictionary != null)
                dictionary.Clear();
            else
                dictionary = new Dictionary<TEnum, TValue>();
            foreach (var key in keys)
            {
                dictionary.Add((TEnum)key, default);
            }
            foreach (var entry in entries)
            {
                var index = Array.IndexOf(keys, Enum.ToObject(typeof(TEnum), entry.key));
                if (index != -1)
                {

                    dictionary[(TEnum)keys.GetValue(index)] = entry.value;

                }
            }
            entries.Clear();

            foreach (var item in entries)
            {


                dictionary[(TEnum)Enum.ToObject(typeof(TEnum), item.key)] = item.value;
            }
            foreach (TEnum item in keys)
            {
                if (!dictionary.ContainsKey(item))
                    dictionary.Add(item, default);
            }

            entries.Clear();
        }

        public void OnBeforeSerialize()
        {

            if (entries.Count > 0)
            {

                foreach (var entry in entries)
                {
                    if (Enum.IsDefined(typeof(TEnum), entry.key) && !EqualityComparer<TValue>.Default.Equals(entry.value, default))
                    {
                        dictionary.Remove((TEnum)Enum.ToObject(typeof(TEnum), entry.key));

                    }
                }

            }
            foreach (var entry in dictionary)
            {

                if (!Enum.IsDefined(typeof(TEnum), entry.Key))
                {
                    continue;
                }
                entries.Add(new Entry(Convert.ToInt32(entry.Key), entry.Value));
            }
            entries.Sort();
            dictionary.Clear();
        }
        [Serializable]
        public class Entry : IComparable<Entry>
        {
            [SerializeField]
            public int key;
            [SerializeField]
            public TValue value;

            public Entry(int key, TValue value)
            {
                this.key = key;
                this.value = value;
            }

            public int CompareTo(Entry other)
            {
                return key.CompareTo(other.key);
            }
        }

        /*         [SerializeField]
                private List<Entry> entries = new List<Entry>(Enum.GetValues(typeof(IEnum)).Length);
                private Dictionary<int, IValue> values = new Dictionary<int, IValue>();

                public IValue this[IEnum key]
                {
                    get => values[Convert.ToInt32(key)];
                    set => values[Convert.ToInt32(key)] = value;
                }
                public void OnAfterDeserialize()
                {

                    if (values != null)
                        values.Clear();
                    else
                        values = new Dictionary<int, IValue>();
                    var validKeys = (IEnum[])Enum.GetValues(typeof(IEnum));
                    foreach (var item in entries)
                    {

                            if (EqualityComparer<IValue>.Default.Equals(item.value, default))
                                values[item.key] = default;
                            else
                                values[item.key] = (IValue)item.value;
                    }
                    foreach (var item in validKeys)
                    {
                        if (!values.ContainsKey(Convert.ToInt32(item)))
                            values.Add(Convert.ToInt32(item), default);
                    }

                    entries.Clear();
                }

                public void OnBeforeSerialize()
                {
                    if (entries.Count > 0)
                    {
                        foreach (var entry in entries)
                        {
                            values[entry.key] = entry.value;
                        }
                    }
                    entries.Clear();
                    var validKeys = (IEnum[])Enum.GetValues(typeof(IEnum));
                    foreach (var item in validKeys)
                    {
                        var key = Convert.ToInt32(item);
                        if (values.TryGetValue(key, out IValue value))
                        {
                            entries.Add(new Entry(key, value));
                        }
                        else
                            entries.Add(new Entry(key, default));

                    }
                    values.Clear();
                    entries.Sort();

                }
                [Serializable]
                private class Entry : IComparable<Entry>
                {
                    public int key;

                    public IValue value;

                    public Entry(int key, IValue value)
                    {
                        this.key = key;
                        this.value = value;
                    }

                    public int CompareTo(Entry other)
                    {
                        return key.CompareTo(other.key);
                    }
                } */
    }



}