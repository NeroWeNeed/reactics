using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace Reactics.Editor
{
    public static class VisualElementProviders
    {
        private static Dictionary<Type, Type> providers;
        private static bool initialized = false;
        static VisualElementProviders()
        {
            Initialize();
        }
        private static void Initialize()
        {
            if (initialized)
                return;
            providers = new Dictionary<Type, Type>();
            CustomVisualElementProvider attr;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    attr = type.GetCustomAttribute<CustomVisualElementProvider>();
                    if (attr != null && typeof(GraphField).IsAssignableFrom(type))
                    {
                        providers[attr.Type] = type;
                    }

                }

            }
            initialized = true;
        }

        public static GraphField Create(Type type, string label = null, object initialValue = default)
        {
            var result = Activator.CreateInstance(providers[type]) as GraphField;
            result.Initialize(label, initialValue);
            return result;
        }
        public static bool TryCreate<V>(Type type, out GraphField<V> result, string label = null, V initialValue = default)
        {
            providers.TryGetValue(type, out Type t);
            if (t == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = Activator.CreateInstance(t) as GraphField<V>;
                result.Initialize(label, initialValue);
                return true;
            }
        }
    }

    public abstract class GraphField : VisualElement
    {

        public abstract void Initialize(string label, object initialValue);


        public abstract void TrySetValue(object value);
        public abstract void TrySetValueWithoutNotify(object value);
        public abstract object GetUntypedValue();

    }
    public abstract class GraphField<V> : GraphField, INotifyValueChanged<V>
    {
        public abstract V value { get; set; }
        public abstract void SetValueWithoutNotify(V newValue);
        public abstract void Initialize(string label, V initialValue);

        public override void Initialize(string label, object initialValue) => Initialize(label, (V)initialValue);
        public override void TrySetValue(object value)
        {

            if (value is V typed)
                this.value = typed;
        }
        public override void TrySetValueWithoutNotify(object value)
        {

            if (value is V typed)
                SetValueWithoutNotify(typed);

        }
        public override object GetUntypedValue()
        {
            return value;
        }
    }


    public abstract class BaseNumberGraphField<TValue> : GraphField<TValue> where TValue : IComparable<TValue>
    {

        public TValue Min { get; protected set; }
        public TValue Max { get; protected set; }
        private TValue _value;

        public override TValue value
        {
            get => _value; set
            {

                if (!value.Equals(_value))
                {

                    if (panel != null)
                    {
                        using (ChangeEvent<TValue> evt = ChangeEvent<TValue>.GetPooled(_value, value))
                        {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            SendEvent(evt);
                            using (ChangeEvent evt2 = ChangeEvent.GetPooled(_value, value))
                            {

                                evt2.target = this;
                                SendEvent(evt2);

                            }
                        }
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }

        public TextField textField { get; protected set; }



        protected BaseNumberGraphField(TValue min, TValue max)
        {
            Min = min;
            Max = max;
        }
        public override void Initialize(string label = null, TValue initial = default)
        {
            textField = new TextField()
            {
                value = initial.ToString(),

            };
            textField.label = label;
            textField.labelElement.style.flexDirection = FlexDirection.RowReverse;
            textField.labelElement.style.unityTextAlign = TextAnchor.UpperRight;
            textField.labelElement.style.minWidth = 0;
            textField.labelElement.style.marginLeft = 4;
            textField.labelElement.style.marginRight = 4;
            textField.style.textOverflow = TextOverflow.Clip;
            var textInput = textField.Q<VisualElement>("unity-text-input");
            textInput.style.maxWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;
            textInput.style.minWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;

            textField.style.flexDirection = FlexDirection.RowReverse;

            textField.RegisterValueChangedCallback((evt) =>
            {
                if (ToValue(evt.newValue, out TValue value))
                {
                    if (value.CompareTo(Min) >= 0 && value.CompareTo(Max) <= 0)
                        this.value = value;
                }
            });
            Add(textField);
        }
        public abstract bool ToValue(string value, out TValue result);
        public virtual string FromValue(TValue value) => value.ToString();
        public override void SetValueWithoutNotify(TValue newValue)
        {
            _value = newValue;
            textField.SetValueWithoutNotify(FromValue(newValue));
        }

    }


    public abstract class DelegateNumberGraphField<TValue> : BaseNumberGraphField<TValue> where TValue : IComparable<TValue>
    {
        public delegate bool Converter<T>(string value, out T result);

        protected Converter<TValue> converter;
        protected DelegateNumberGraphField(TValue min, TValue max, Converter<TValue> converter) : base(min, max)
        {
            this.converter = converter;
        }

        public override bool ToValue(string value, out TValue result) => converter(value, out result);
    }


    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CustomVisualElementProvider : Attribute
    {
        public CustomVisualElementProvider(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }
    }

    public abstract class NumericGraphField<TValue, TElement, TElementValue> : GraphField<TValue> where TValue : IComparable<TValue> where TElement : VisualElement, INotifyValueChanged<TElementValue> where TElementValue : IComparable<TElementValue>
    {
        public TValue MinValue { get; protected set; }
        public TValue MaxValue { get; protected set; }

        protected TValue _value;
        public override TValue value
        {
            get => _value; set
            {

                if (!value.Equals(_value))
                {

                    if (panel != null)
                    {

                        using (ChangeEvent<TValue> evt = ChangeEvent<TValue>.GetPooled(_value, value))
                        {
                            using (ChangeEvent<object> evt2 = ChangeEvent<object>.GetPooled(_value, value))
                            {
                                evt.target = this;
                                SetValueWithoutNotify(value);
                                SendEvent(evt);
                                evt2.target = this;
                                SendEvent(evt2);
                            }
                        }



                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }

            }
        }

        protected TElement element;

        public NumericGraphField(TValue min, TValue max)
        {

            this.MinValue = min;
            this.MaxValue = max;
        }
        public override void SetValueWithoutNotify(TValue newValue)
        {
            _value = newValue;
            SetValue(element, newValue);
        }

        protected abstract TElement CreateElement(string label, TValue initialValue);
        protected abstract TValue GetValue(TElementValue elementValue);
        protected abstract void SetValue(TElement element, TValue value);
        public override void Initialize(string label, TValue initialValue)
        {
            if (element != null)
                element.RemoveFromHierarchy();
            element = CreateElement(label, initialValue);
            element.RegisterCallback<ChangeFieldEvent>((evt) =>
            {
                Debug.Log("CHANGE FIELD SIGNALED");
            });
            element.RegisterValueChangedCallback((evt) =>
            {

                var convertedValue = GetValue(evt.newValue);
                Debug.Log(convertedValue);
                if (convertedValue.CompareTo(MinValue) < 0)
                {
                    value = MinValue;
                    SetValue(element, MinValue);

                }
                else if (convertedValue.CompareTo(MaxValue) > 0)
                {
                    value = MaxValue;
                    SetValue(element, MaxValue);
                }
                else
                {
                    value = convertedValue;
                }
            });
            this.Add(element);
        }

    }
    //Base unmanaged providers
    [CustomVisualElementProvider(typeof(sbyte))]
    public class SbyteGraphField : DelegateNumberGraphField<sbyte> { public SbyteGraphField() : base(sbyte.MinValue, sbyte.MaxValue, (string value, out sbyte result) => sbyte.TryParse(value, out result)) { } }
    [CustomVisualElementProvider(typeof(byte))]
    public class ByteGraphField : DelegateNumberGraphField<byte> { public ByteGraphField() : base(byte.MinValue, byte.MaxValue, (string value, out byte result) => byte.TryParse(value, out result)) { } }
    [CustomVisualElementProvider(typeof(short))]
    public class ShortGraphField : DelegateNumberGraphField<short> { public ShortGraphField() : base(short.MinValue, short.MaxValue, (string value, out short result) => short.TryParse(value, out result)) { } }
    [CustomVisualElementProvider(typeof(ushort))]
    public class UshortGraphField : DelegateNumberGraphField<ushort> { public UshortGraphField() : base(ushort.MinValue, ushort.MaxValue, (string value, out ushort result) => ushort.TryParse(value, out result)) { } }
    [CustomVisualElementProvider(typeof(int))]
    public class IntGraphField : DelegateNumberGraphField<int> { public IntGraphField() : base(int.MinValue, int.MaxValue, (string value, out int result) => int.TryParse(value, out result)) { } }
    [CustomVisualElementProvider(typeof(uint))]
    public class UintGraphField : DelegateNumberGraphField<uint> { public UintGraphField() : base(uint.MinValue, uint.MaxValue, (string value, out uint result) => uint.TryParse(value, out result)) { } }
    [CustomVisualElementProvider(typeof(long))]
    public class LongGraphField : DelegateNumberGraphField<long> { public LongGraphField() : base(long.MinValue, long.MaxValue, (string value, out long result) => long.TryParse(value, out result)) { } }
    [CustomVisualElementProvider(typeof(ulong))]
    public class UlongGraphField : DelegateNumberGraphField<ulong> { public UlongGraphField() : base(ulong.MinValue, ulong.MaxValue, (string value, out ulong result) => ulong.TryParse(value, out result)) { } }
    /* 
        [CustomVisualElementProvider(typeof(sbyte))]
        public class SByteGraphField : NumericGraphField<sbyte, IntegerField, int>
        {
            public SByteGraphField() : base(sbyte.MinValue, sbyte.MaxValue) { }
            protected override IntegerField CreateElement(string label, sbyte initialValue) => new IntegerField(label) { value = initialValue };
            protected override sbyte GetValue(int elementValue) => Convert.ToSByte(elementValue);

            protected override void SetValue(IntegerField element, sbyte value) { element.value = value; }
        }
        [CustomVisualElementProvider(typeof(byte))]
        public class ByteGraphField : NumericGraphField<byte, IntegerField, int>
        {
            public ByteGraphField() : base(byte.MinValue, byte.MaxValue) { }
            protected override IntegerField CreateElement(string label, byte initialValue) => new IntegerField(label) { value = initialValue };
            protected override byte GetValue(int elementValue) => Convert.ToByte(elementValue);

            protected override void SetValue(IntegerField element, byte value) { element.value = value; }
        }
        [CustomVisualElementProvider(typeof(short))]
        public class ShortGraphField : NumericGraphField<short, IntegerField, int>
        {
            public ShortGraphField() : base(short.MinValue, short.MaxValue) { }
            protected override IntegerField CreateElement(string label, short initialValue) => new IntegerField(label) { value = initialValue };
            protected override short GetValue(int elementValue) => Convert.ToInt16(elementValue);

            protected override void SetValue(IntegerField element, short value) { element.value = value; }
        }
        [CustomVisualElementProvider(typeof(ushort))]
        public class UShortGraphField : NumericGraphField<ushort, IntegerField, int>
        {
            public UShortGraphField() : base(ushort.MinValue, ushort.MaxValue) { }
            protected override IntegerField CreateElement(string label, ushort initialValue) => new IntegerField(label) { value = initialValue };
            protected override ushort GetValue(int elementValue) => Convert.ToUInt16(elementValue);

            protected override void SetValue(IntegerField element, ushort value) { element.value = value; }
        }



        [CustomVisualElementProvider(typeof(int))]
        public class IntGraphField : NumericGraphField<int, IntegerField, int>
        {
            public IntGraphField() : base(int.MinValue, int.MaxValue)
            {
            }

            protected override IntegerField CreateElement(string label, int initialValue) => new IntegerField(label)
            {
                value = initialValue
            };

            protected override int GetValue(int elementValue) => elementValue;

            protected override void SetValue(IntegerField element, int value) { element.value = value; }
        }
        [CustomVisualElementProvider(typeof(uint))]
        public class UIntGraphField : NumericGraphField<uint, LongField, long>
        {
            public UIntGraphField() : base(uint.MinValue, uint.MaxValue)
            {
            }

            protected override LongField CreateElement(string label, uint initialValue) => new LongField(label)
            {
                value = initialValue
            };

            protected override uint GetValue(long elementValue) => Convert.ToUInt32(elementValue);

            protected override void SetValue(LongField element, uint value) { element.value = value; }
        }
        [CustomVisualElementProvider(typeof(long))]
        public class LongGraphField : NumericGraphField<long, LongField, long>
        {
            public LongGraphField() : base(long.MinValue, long.MaxValue)
            {
            }

            protected override LongField CreateElement(string label, long initialValue) => new LongField(label)
            {
                value = initialValue
            };

            protected override long GetValue(long elementValue) => elementValue;

            protected override void SetValue(LongField element, long value) { element.value = value; }
        }
     */
}