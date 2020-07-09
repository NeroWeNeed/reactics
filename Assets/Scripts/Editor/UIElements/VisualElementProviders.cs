using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using Reactics.Commons;
using UnityEngine.AddressableAssets;
using Reactics.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using System.Numerics;

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
                    if (attr != null && typeof(VisualElementDrawer).IsAssignableFrom(type))
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
        public static VisualElementDrawer Create(Type type, string label = null, object initialValue = default, Attribute[] attributes = null)
        {
            var t = FindType(type);
            if (t == null)
                throw new ArgumentException("Invalid Type");
            var result = Activator.CreateInstance(providers[t]) as VisualElementDrawer;
            result.Initialize(label, initialValue, attributes);
            return result;
        }
        public static VisualElement CreateGraphFieldPort(Type type, string label = null, object initialValue = default, Attribute[] attributes = null)
        {
            var t = FindType(type);
            if (t == null)
                throw new ArgumentException("Invalid Type");
            var element = Activator.CreateInstance(providers[t]) as VisualElementDrawer;
            if (t.GetCustomAttribute<StandaloneField>() == null)
            {
                var o = Activator.CreateInstance(typeof(ObjectGraphValuePort<>).MakeGenericType(type)) as VisualElementDrawer;
                o.GetType().GetMethod("SetPillElement").MakeGenericMethod(element.GetType()).Invoke(o, new object[] { element });
                o.Initialize(label, initialValue, attributes);
                return o;
            }
            else
            {
                element.Initialize(label, initialValue, attributes);
                return element;
            }
        }

        public static bool TryCreate<V>(Type type, out VisualElementDrawer<V> result, string label = null, V initialValue = default, Attribute[] attributes = null)
        {
            var t = FindType(type);

            if (t == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = Activator.CreateInstance(providers[t]) as VisualElementDrawer<V>;
                result.Initialize(label, initialValue, attributes);
                return true;
            }
        }
    }

    public abstract class VisualElementDrawer : VisualElement
    {

        public abstract void Initialize(string label, object initialValue, Attribute[] attributes = null);


        public abstract void TrySetValue(object value);
        public abstract void TrySetValueWithoutNotify(object value);
        public abstract object GetUntypedValue();

    }
    public abstract class VisualElementDrawer<TValue> : VisualElementDrawer, INotifyValueChanged<TValue>
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
        public virtual void SetValueWithoutNotify(TValue newValue)
        {
            _value = newValue;
        }
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

    public abstract class BaseTextValueGraphField<TValue> : VisualElementDrawer<TValue> where TValue : IComparable<TValue>
    {
        public delegate bool Converter<T>(string value, out T result);

        public BaseTextValueGraphField(Converter<TValue> converter)
        {
            this.converter = converter;
        }
        protected Converter<TValue> converter;
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

            //textField.style.flexDirection = FlexDirection.RowReverse;
            textField.RegisterValueChangedCallback(OnValueChangedEvent);

            Add(textField);
        }

        protected virtual void OnValueChangedEvent(ChangeEvent<string> evt)
        {

            if (converter(evt.newValue, out TValue value) && IsValueValid(value))
                this.value = value;
            else
            {
                var t = AdjustValue(evt.newValue, evt.previousValue);
                this.value = t;
                SetValueWithoutNotify(t);
            }

        }
        protected virtual bool IsValueValid(TValue value) => true;

        protected virtual TValue AdjustValue(string targetValue, string previousValue) => default;
        public override void SetValueWithoutNotify(TValue newValue)
        {
            _value = newValue;
            textField.SetValueWithoutNotify(FromValue(newValue));
        }

    }
    public abstract class NumericTextValueGraphField<TValue> : BaseTextValueGraphField<TValue> where TValue : IComparable<TValue>
    {
        public static readonly System.Globalization.NumberStyles FloatingPointStyle = System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowExponent;
        public static readonly System.Globalization.NumberStyles IntegerPointStyle = System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowExponent;


        protected NumericTextValueGraphField(Converter<TValue> converter) : base(converter) { }

        protected virtual TValue Approximate(TValue old) => old;
        public override void Initialize(string label = null, TValue initial = default, Attribute[] attributes = null)
        {
            base.Initialize(label, initial, attributes);
        }
        protected override void OnValueChangedEvent(ChangeEvent<string> evt)
        {

            if (converter(evt.newValue, out TValue value) && IsValueValid(value))
            {
                this.value = value;
            }
            else
            {
                var t = AdjustValue(evt.newValue, evt.previousValue);
                this.value = t;
                SetValueWithoutNotify(t);
            }


        }

    }
    public abstract class RestrictedNumberGraphField<TValue> : NumericTextValueGraphField<TValue> where TValue : IComparable<TValue>
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

        /*         
         */
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
    public class SbyteGraphField : NumericTextValueGraphField<sbyte> { public SbyteGraphField() : base((string value, out sbyte result) => sbyte.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(byte))]
    public class ByteGraphField : NumericTextValueGraphField<byte>
    {
        public ByteGraphField() : base((string value, out byte result) => byte.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { }

    }
    [CustomVisualElementProvider(typeof(short))]
    public class ShortGraphField : NumericTextValueGraphField<short> { public ShortGraphField() : base((string value, out short result) => short.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(ushort))]
    public class UshortGraphField : NumericTextValueGraphField<ushort> { public UshortGraphField() : base((string value, out ushort result) => ushort.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(int))]
    public class IntGraphField : NumericTextValueGraphField<int> { public IntGraphField() : base((string value, out int result) => int.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(uint))]
    public class UintGraphField : NumericTextValueGraphField<uint> { public UintGraphField() : base((string value, out uint result) => uint.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(long))]
    public class LongGraphField : NumericTextValueGraphField<long> { public LongGraphField() : base((string value, out long result) => long.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(ulong))]
    public class UlongGraphField : NumericTextValueGraphField<ulong> { public UlongGraphField() : base((string value, out ulong result) => ulong.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(float))]
    public class FloatGraphField : NumericTextValueGraphField<float> { public FloatGraphField() : base((string value, out float result) => float.TryParse(value, FloatingPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(double))]
    public class DoubleGraphField : NumericTextValueGraphField<double> { public DoubleGraphField() : base((string value, out double result) => double.TryParse(value, FloatingPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(char))]
    public class CharGraphField : NumericTextValueGraphField<char>
    {
        public CharGraphField() : base((string value, out char result) => char.TryParse(value, out result)) { }
        public override void Initialize(string label = null, char initial = default, Attribute[] attributes = null)
        {
            base.Initialize(label, initial);
            textField.maxLength = 1;
            var textInput = textField.Q<VisualElement>("unity-text-input");
            textInput.style.minWidth = textInput.style.maxWidth = 11;
        }
    }
    [CustomVisualElementProvider(typeof(bool))]
    public class BoolGraphField : VisualElementDrawer<bool>
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


            Add(toggle);
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            _value = newValue;
            toggle.SetValueWithoutNotify(newValue);
        }
    }
    [CustomVisualElementProvider(typeof(Enum))]
    public class EnumGraphField : VisualElementDrawer<Enum>
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


            enumField.RegisterValueChangedCallback((evt) => this.value = evt.newValue);
            Add(enumField);
        }

        public override void SetValueWithoutNotify(Enum newValue)
        {
            _value = newValue;
            enumField.SetValueWithoutNotify(newValue);
        }
    }
    [CustomVisualElementProvider(typeof(BlittableAssetReference64))]
    public class AssetReferenceGraphField : VisualElementDrawer<BlittableAssetReference64>
    {
        private AssetReferenceSearchField searchField;
        public override void Initialize(string label, BlittableAssetReference64 initialValue, Attribute[] attributes = null)
        {
            searchField = new AssetReferenceSearchField(label);
            this.SetValueWithoutNotify(initialValue);
            searchField.RegisterValueChangedCallback(OnValueInSearchFieldChanged);
            this.Add(searchField);
        }
        private void OnValueInSearchFieldChanged(ChangeEvent<AssetReference> evt)
        {
            if (evt.newValue != null && !string.IsNullOrEmpty(evt.newValue.SubObjectName) && evt.newValue.SubObjectName.Length > BlittableAssetReference64.SubObjectNameMaxLength)
            {
                Debug.LogError($"Selected SubObject of Asset must have a name less than or equal to {BlittableAssetReference64.SubObjectNameMaxLength}");
                return;
            }
            value = (BlittableAssetReference64)evt.newValue;
        }

    }

}