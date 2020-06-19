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
        private static Type FindType(Type type)
        {
            if (providers.ContainsKey(type))
            {
                return type;
            }
            else
            {
                if (type.BaseType != null)
                {
                    return FindType(type.BaseType);
                }
                else
                    return null;
            }
        }
        public static GraphField Create(Type type, string label = null, object initialValue = default, Attribute[] attributes = null)
        {
            var t = FindType(type);
            if (t == null)
                throw new ArgumentException("Invalid Type");
            var result = Activator.CreateInstance(providers[t]) as GraphField;
            result.Initialize(label, initialValue, attributes);
            return result;
        }
        public static bool TryCreate<V>(Type type, out GraphField<V> result, string label = null, V initialValue = default, Attribute[] attributes = null)
        {
            var t = FindType(type);

            if (t == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = Activator.CreateInstance(providers[t]) as GraphField<V>;
                result.Initialize(label, initialValue, attributes);
                return true;
            }
        }
    }

    public abstract class GraphField : VisualElement
    {

        public abstract void Initialize(string label, object initialValue, Attribute[] attributes = null);


        public abstract void TrySetValue(object value);
        public abstract void TrySetValueWithoutNotify(object value);
        public abstract object GetUntypedValue();

    }
    public abstract class GraphField<TValue> : GraphField, INotifyValueChanged<TValue>
    {
        protected TValue _value;
        public TValue value
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
                        }
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }
        public abstract void SetValueWithoutNotify(TValue newValue);
        public abstract void Initialize(string label, TValue initialValue, Attribute[] attributes = null);

        public override void Initialize(string label, object initialValue, Attribute[] attributes = null) => Initialize(label, (TValue)initialValue, attributes);
        public override void TrySetValue(object value)
        {

            if (value is TValue typed)
                this.value = typed;
        }
        public override void TrySetValueWithoutNotify(object value)
        {

            if (value is TValue typed)
                SetValueWithoutNotify(typed);

        }
        public override object GetUntypedValue()
        {
            return value;
        }
    }

    public abstract class BaseTextValueGraphField<TValue> : GraphField<TValue> where TValue : IComparable<TValue>
    {
        public delegate bool Converter<T>(string value, out T result);

        public BaseTextValueGraphField(Converter<TValue> converter)
        {
            this.converter = converter;
        }
        private Converter<TValue> converter;
        public TextField textField { get; protected set; }
        public virtual bool ToValue(string value, out TValue result) => converter(value, out result);
        public virtual string FromValue(TValue value) => value.ToString();
        public override void Initialize(string label = null, TValue initial = default, Attribute[] attributes = null)
        {
            textField = new TextField()
            {
                value = initial.ToString(),
            };
            textField.label = label;
            //textField.labelElement.style.flexDirection = FlexDirection.RowReverse;
            textField.labelElement.style.unityTextAlign = TextAnchor.UpperRight;
            textField.labelElement.style.minWidth = 0;
            textField.labelElement.style.marginLeft = 4;
            textField.labelElement.style.marginRight = 4;
            textField.style.textOverflow = TextOverflow.Clip;
            //var textInput = textField.Q<VisualElement>("unity-text-input");
            //textInput.style.maxWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;
            //textInput.style.minWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;

            textField.style.flexDirection = FlexDirection.RowReverse;
            textField.RegisterValueChangedCallback(OnValueChangedEvent);
            Add(textField);
        }

        protected virtual void OnValueChangedEvent(ChangeEvent<string> evt)
        {

            if (converter(evt.newValue, out TValue value))
            {
                if (IsValueValid(value))
                    this.value = value;
            }
        }
        protected virtual bool IsValueValid(TValue value) => true;
        public override void SetValueWithoutNotify(TValue newValue)
        {
            _value = newValue;
            textField.SetValueWithoutNotify(FromValue(newValue));
        }

    }
    public abstract class NumericTextValueGraphField<TValue> : BaseTextValueGraphField<TValue> where TValue : IComparable<TValue>
    {
        public static readonly System.Globalization.NumberStyles FloatingPointStyle = System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowExponent;
        public static readonly System.Globalization.NumberStyles IntegerPointStyle = System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowExponent | System.Globalization.NumberStyles.AllowHexSpecifier;
        public System.Globalization.NumberStyles Styles { get; protected set; }

        protected NumericTextValueGraphField(System.Globalization.NumberStyles styles, Converter<TValue> converter) : base(converter)
        {
            this.Styles = styles;
        }
        protected NumericTextValueGraphField(Converter<TValue> converter) : this(System.Globalization.NumberStyles.None, converter)
        {
        }
    }
    public abstract class RestrictedNumberGraphField<TValue> : BaseTextValueGraphField<TValue> where TValue : IComparable<TValue>
    {

        public TValue Min { get; protected set; }
        public TValue Max { get; protected set; }

        protected RestrictedNumberGraphField(TValue min, TValue max, Converter<TValue> converter) : base(converter)
        {
            Min = min;
            Max = max;
        }
        public override void Initialize(string label = null, TValue initial = default, Attribute[] attributes = null)
        {
            base.Initialize(label, initial);
            var textInput = textField.Q<VisualElement>("unity-text-input");
            textInput.style.maxWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 1) * 11;
            textInput.style.minWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 1) * 11;
        }
        /*         public override void Initialize(string label = null, TValue initial = default)
                {
                    textField = new TextField()
                    {
                        value = initial.ToString(),
                    };
                    textField.label = label;
                    //textField.labelElement.style.flexDirection = FlexDirection.RowReverse;
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
                } */



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
    //Base unmanaged providers
    [CustomVisualElementProvider(typeof(sbyte))]
    public class SbyteGraphField : NumericTextValueGraphField<sbyte> { public SbyteGraphField() : base(IntegerPointStyle, (string value, out sbyte result) => sbyte.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(byte))]
    public class ByteGraphField : NumericTextValueGraphField<byte> { public ByteGraphField() : base(IntegerPointStyle, (string value, out byte result) => byte.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(short))]
    public class ShortGraphField : NumericTextValueGraphField<short> { public ShortGraphField() : base(IntegerPointStyle, (string value, out short result) => short.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(ushort))]
    public class UshortGraphField : NumericTextValueGraphField<ushort> { public UshortGraphField() : base(IntegerPointStyle, (string value, out ushort result) => ushort.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(int))]
    public class IntGraphField : NumericTextValueGraphField<int> { public IntGraphField() : base(IntegerPointStyle, (string value, out int result) => int.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(uint))]
    public class UintGraphField : NumericTextValueGraphField<uint> { public UintGraphField() : base(IntegerPointStyle, (string value, out uint result) => uint.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(long))]
    public class LongGraphField : NumericTextValueGraphField<long> { public LongGraphField() : base(IntegerPointStyle, (string value, out long result) => long.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(ulong))]
    public class UlongGraphField : NumericTextValueGraphField<ulong> { public UlongGraphField() : base(IntegerPointStyle, (string value, out ulong result) => ulong.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(float))]
    public class FloatGraphField : NumericTextValueGraphField<float> { public FloatGraphField() : base(FloatingPointStyle, (string value, out float result) => float.TryParse(value, FloatingPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(double))]
    public class DoubleGraphField : NumericTextValueGraphField<double> { public DoubleGraphField() : base(FloatingPointStyle, (string value, out double result) => double.TryParse(value, FloatingPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(char))]
    public class CharGraphField : NumericTextValueGraphField<char>
    {
        public CharGraphField() : base(FloatingPointStyle, (string value, out char result) => char.TryParse(value, out result)) { }
        public override void Initialize(string label = null, char initial = default, Attribute[] attributes = null)
        {
            base.Initialize(label, initial);
            textField.maxLength = 1;
            var textInput = textField.Q<VisualElement>("unity-text-input");
            textInput.style.minWidth = textInput.style.maxWidth = 11;
        }
    }
    [CustomVisualElementProvider(typeof(bool))]
    public class BoolGraphField : GraphField<bool>
    {
        public bool value
        {
            get => _value; set
            {

                if (!value.Equals(_value))
                {

                    if (panel != null)
                    {
                        using (ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(_value, value))
                        {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            SendEvent(evt);
                        }
                    }
                    else
                    {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }
        public Toggle toggle { get; protected set; }
        public override void Initialize(string label, bool initialValue, Attribute[] attributes = null)
        {

            toggle = new Toggle(label)
            {
                value = initialValue
            };
            //textField.labelElement.style.flexDirection = FlexDirection.RowReverse;
            toggle.labelElement.style.unityTextAlign = TextAnchor.UpperRight;
            toggle.labelElement.style.minWidth = 0;
            toggle.labelElement.style.marginLeft = 4;
            toggle.labelElement.style.marginRight = 4;
            toggle.style.textOverflow = TextOverflow.Clip;
            //var textInput = textField.Q<VisualElement>("unity-text-input");
            //textInput.style.maxWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;
            //textInput.style.minWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;
            toggle.RegisterValueChangedCallback((evt) => this.value = evt.newValue);
            toggle.RegisterCallback<AttachToPanelEvent>((evt) =>
            {
                if (evt.target is VisualElement visualElement && visualElement.parent != null)
                    visualElement.viewDataKey = visualElement.parent.viewDataKey;
            });
            toggle.style.flexDirection = FlexDirection.RowReverse;

            Add(toggle);
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            _value = newValue;
            toggle.SetValueWithoutNotify(newValue);
        }
    }
    [CustomVisualElementProvider(typeof(Enum))]
    public class EnumGraphField : GraphField<Enum>
    {
        public EnumField enumField { get; protected set; }
        public override void Initialize(string label, Enum initialValue, Attribute[] attributes = null)
        {
            enumField = new EnumField(label, initialValue);


            //textField.labelElement.style.flexDirection = FlexDirection.RowReverse;
            enumField.labelElement.style.unityTextAlign = TextAnchor.UpperRight;
            enumField.labelElement.style.minWidth = 0;
            enumField.labelElement.style.marginLeft = 4;
            enumField.labelElement.style.marginRight = 4;
            enumField.style.textOverflow = TextOverflow.Clip;
            //var textInput = textField.Q<VisualElement>("unity-text-input");
            //textInput.style.maxWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;
            //textInput.style.minWidth = (Math.Max(Min.ToString().Length, Max.ToString().Length) + 2) * 11;

            enumField.style.flexDirection = FlexDirection.RowReverse;
            enumField.RegisterValueChangedCallback((evt) => this.value = evt.newValue);
            Add(enumField);
        }

        public override void SetValueWithoutNotify(Enum newValue)
        {
            _value = newValue;
            enumField.SetValueWithoutNotify(newValue);
        }
    }


}