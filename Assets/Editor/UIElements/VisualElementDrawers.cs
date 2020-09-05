using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Reactics.Core.Commons;
using Reactics.Core.Commons.Reflection;
using Reactics.Editor.Graph;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Reactics.Editor {
    public static class VisualElementDrawers {
        private static Dictionary<Type, Type> providers;
        private static bool initialized = false;
        static VisualElementDrawers() {
            Initialize();
        }
        private static void Initialize() {
            if (initialized)
                return;
            providers = new Dictionary<Type, Type>();
            CustomVisualElementProvider attr;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (var type in assembly.GetTypes()) {
                    attr = type.GetCustomAttribute<CustomVisualElementProvider>();
                    if (attr != null && typeof(VisualElementDrawer).IsAssignableFrom(type)) {
                        providers[attr.Type] = type;
                    }
                }
            }
            initialized = true;
        }
        private static Type FindType(Type type) {
            if (providers.ContainsKey(type)) {
                return type;
            }
            else {
                if (type.BaseType != null) {
                    return FindType(type.BaseType);
                }
                else {
                    return null;
                }
            }
        }
        public static VisualElementDrawer Create(Type type, string label = null, object initialValue = default, Attribute[] attributes = null) {
            var t = FindType(type);
            if (t == null)
                throw new ArgumentException("Invalid Type");
            var result = Activator.CreateInstance(providers[t]) as VisualElementDrawer;
            result.Initialize(label, initialValue, attributes);

            return result;
        }
        public static VisualElement CreateInspector(SerializedObject obj, string label) {
            VisualElement inspector = new VisualElement();
            var original = obj?.targetObject;
            if (original == null)
                return inspector;
            var iter = obj.GetIterator();
            while (iter.NextVisible(true)) {
                Debug.Log(iter.propertyType);
                Debug.Log(iter.propertyPath);
            }

            foreach (var field in original.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where((f) => f.IsSerializableField())) {
                var prop = obj.FindProperty(field.Name);
                if (prop != null) {
                    inspector.Add(new PropertyField(prop));
                }
            }
            inspector.Bind(obj);
            return inspector;
        }

        public static bool TryCreate<V>(Type type, out VisualElementDrawer<V> result, string label = null, V initialValue = default, Attribute[] attributes = null) {
            var t = FindType(type);

            if (t == null) {
                result = null;
                return false;
            }
            else {
                result = Activator.CreateInstance(providers[t]) as VisualElementDrawer<V>;
                result.Initialize(label, initialValue, attributes);
                return true;
            }
        }
        public static bool TryCreate(Type type, out VisualElementDrawer result, string label = null, object initialValue = default, Attribute[] attributes = null) {
            var t = FindType(type);

            if (t == null) {
                result = null;
                return false;
            }
            else {
                result = Activator.CreateInstance(providers[t]) as VisualElementDrawer;
                result.Initialize(label, initialValue, attributes);
                return true;
            }
        }
    }

    public abstract class VisualElementDrawer : BindableElement {

        public abstract void Initialize(string label, object initialValue, Attribute[] attributes = null);

        public abstract string Label { get; set; }
        public abstract void TrySetValue(object value);
        public abstract void TrySetValueWithoutNotify(object value);
        public abstract object GetUntypedValue();

        public abstract Type GetValueType();
        public abstract void RegisterOnValueChanged(Action<object, object> callback);

    }
    public abstract class VisualElementDrawer<TValue> : VisualElementDrawer, INotifyValueChanged<TValue> {
        protected TValue _value;
        public TValue value
        {
            get => _value; set
            {

                if (!value.Equals(_value)) {

                    if (panel != null) {
                        using (ChangeEvent<TValue> evt = ChangeEvent<TValue>.GetPooled(_value, value)) {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            SendEvent(evt);
                        }
                    }
                    else {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }
        public virtual void SetValueWithoutNotify(TValue newValue) {
            Debug.Log($"SETTING {this.viewDataKey} TO {newValue}");
            _value = newValue;
        }
        public abstract void Initialize(string label, TValue initialValue, Attribute[] attributes = null);

        public override void Initialize(string label, object initialValue, Attribute[] attributes = null) => Initialize(label, (TValue)initialValue, attributes);
        public override void TrySetValue(object value) {

            if (value is TValue typed)
                this.value = typed;
        }
        public override void TrySetValueWithoutNotify(object value) {

            if (value is TValue typed)
                SetValueWithoutNotify(typed);

        }
        public override object GetUntypedValue() {
            return value;
        }
        public override Type GetValueType() => typeof(TValue);
        public virtual void RegisterValueChanged(Action<TValue, TValue> callback) => this.RegisterValueChangedCallback((evt) => callback(evt.previousValue, evt.newValue));
        public override void RegisterOnValueChanged(Action<object, object> callback) {
            this.RegisterValueChangedCallback((evt) => callback(evt.previousValue, evt.newValue));
        }
    }
    [InputLayout]
    public abstract class BaseTextValueDrawer<TValue> : VisualElementDrawer<TValue> {
        public delegate bool Converter<T>(string value, out T result);

        protected BaseTextValueDrawer(Converter<TValue> converter) {
            this.converter = converter;
        }
        protected Converter<TValue> converter;
        protected TValue updateValue;
        public TextField textField { get; protected set; }
        public override string Label { get => textField.label; set => textField.label = value; }
        public virtual bool ToValue(string value, out TValue result) => converter(value, out result);
        public virtual string FromValue(TValue value) => value.ToString();
        public override void Initialize(string label = null, TValue initial = default, Attribute[] attributes = null) {

            textField = new TextField();

            textField.label = label;
            textField.labelElement.style.unityTextAlign = TextAnchor.UpperRight;
            textField.labelElement.style.minWidth = 0;
            textField.labelElement.style.marginLeft = 4;
            textField.labelElement.style.marginRight = 4;
            textField.style.textOverflow = TextOverflow.Clip;
            textField.RegisterValueChangedCallback(OnValueChangedEvent);
            this.RegisterCallback<FocusOutEvent>((_) => PushUpdateValue());
            this.RegisterCallback<KeyDownEvent>((evt) =>
            {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) {
                    PushUpdateValue();
                }
            });
            Add(textField);
            SetValueWithoutNotify(initial);
        }
        protected virtual void PushUpdateValue() {
            value = updateValue;
            SetValueWithoutNotify(updateValue);
        }
        protected virtual void OnValueChangedEvent(ChangeEvent<string> evt) {

            if (converter(evt.newValue, out TValue value) && IsValueValid(value)) {
                updateValue = value;
            }
            else {
                updateValue = AdjustValue(evt.newValue, evt.previousValue);
            }
        }
        protected virtual bool IsValueValid(TValue value) => true;
        protected virtual TValue GetDefaultValue() => default;
        protected virtual TValue AdjustValue(string targetValue, string previousValue) => GetDefaultValue();

        protected virtual TValue AdjustValue(TValue value) => value;
        public override void SetValueWithoutNotify(TValue newValue) {
            if (IsValueValid(newValue)) {
                _value = newValue;
            }
            else {
                var t = AdjustValue(newValue);
                if (IsValueValid(t)) {
                    _value = t;
                }
                else {
                    _value = GetDefaultValue();
                }

            }
            updateValue = _value;
            textField.SetValueWithoutNotify(FromValue(_value));
        }


    }
    public abstract class NumericTextValueDrawer<TValue> : BaseTextValueDrawer<TValue> where TValue : IComparable<TValue> {
        public static readonly System.Globalization.NumberStyles FloatingPointStyle = System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowExponent;
        public static readonly System.Globalization.NumberStyles IntegerPointStyle = System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite | System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowThousands | System.Globalization.NumberStyles.AllowExponent;

        public TValue Min, Max;
        public bool testMin, testMax;
        protected NumericTextValueDrawer(Converter<TValue> converter) : base(converter) { }

        public override void Initialize(string label = null, TValue initial = default, Attribute[] attributes = null) {
            if (attributes != null) {
                ProcessAttributes(attributes);
            }
            base.Initialize(label, initial, attributes);


        }
        protected void ProcessAttributes(Attribute[] attributes) {

            foreach (var attr in attributes) {
                if (attr is RangeAttribute range) {
                    ProcessAttribute(range);
                }
                else if (attr is MinAttribute min) {
                    ProcessAttribute(min);
                }
                else if (attr is MaxAttribute max) {
                    ProcessAttribute(max);
                }

            }
        }
        protected void ProcessAttribute(RangeAttribute range) {
            if (typeof(TValue).IsNumericType()) {
                Min = (TValue)Convert.ChangeType(range.min, typeof(TValue));
                Max = (TValue)Convert.ChangeType(range.max, typeof(TValue));
                testMax = true;
                testMin = true;
            }
        }
        protected void ProcessAttribute(MinAttribute range) {
            if (typeof(TValue).IsNumericType()) {
                Min = (TValue)Convert.ChangeType(range.min, typeof(TValue));
                testMin = true;
            }
        }
        protected void ProcessAttribute(MaxAttribute range) {
            if (typeof(TValue).IsNumericType()) {
                Max = (TValue)Convert.ChangeType(range.max, typeof(TValue));
                testMax = true;
            }
        }
        protected virtual TValue Approximate(TValue old) => old;
        protected override void OnValueChangedEvent(ChangeEvent<string> evt) {

            if (converter(evt.newValue, out TValue value) && IsValueValid(value)) {
                updateValue = value;
            }
            else {
                var t = AdjustValue(evt.newValue, evt.previousValue);
                this.updateValue = t;

            }
        }
        protected override TValue GetDefaultValue() {

            if (testMin && Min.CompareTo(default) >= 0)
                return Min;
            if (testMax && Max.CompareTo(default) <= 0)
                return Max;
            return default;
        }
        protected override bool IsValueValid(TValue value) {
            if (testMin && Min.CompareTo(value) > 0)
                return false;
            if (testMax && Max.CompareTo(value) < 0)
                return false;
            return true;
        }

        protected override TValue AdjustValue(string targetValue, string previousValue) {
            if (ToValue(targetValue, out TValue result)) {
                return AdjustValue(result);
            }
            else {
                return GetDefaultValue();
            }
        }
        protected override TValue AdjustValue(TValue value) {
            if (testMin && Min.CompareTo(value) >= 0)
                return Min;
            if (testMax && Max.CompareTo(value) <= 0)
                return Max;
            return value;

        }
    }

    public abstract class TextValueDrawer<TValue> : BaseTextValueDrawer<TValue> {
        private int maxLength;
        private string updateValue;
        private string stringValue;
        protected TextValueDrawer(Converter<TValue> converter) : base(converter) {
        }
        public override void Initialize(string label = null, TValue initial = default, Attribute[] attributes = null) {
            base.Initialize(label, initial, attributes);
            if (attributes != null) {
                ProcessAttributes(attributes);
            }


        }
        protected void ProcessAttributes(Attribute[] attributes) {

            foreach (var attr in attributes) {
                if (attr is MultilineAttribute multiline) {
                    ProcessAttribute(multiline);
                }
                else if (attr is MaxAttribute max) {
                    ProcessAttribute(max);
                }
            }
        }
        protected void ProcessAttribute(MultilineAttribute attribute) {
            textField.multiline = true;
            textField.style.minHeight = EditorGUIUtility.singleLineHeight * attribute.lines;
        }
        protected void ProcessAttribute(MaxAttribute attribute) {
            maxLength = (int)attribute.max;
        }
        protected override void PushUpdateValue() {
            if (IsValueValid(updateValue) && ToValue(updateValue, out TValue result)) {
                value = result;
            }
            else {
                var t = AdjustValue(updateValue, null);
                if (IsValueValid(t)) {
                    value = t;
                }
                else {
                    value = GetDefaultValue();
                }
            }
            SetValueWithoutNotify(value);
        }
        protected override void OnValueChangedEvent(ChangeEvent<string> evt) {
            stringValue = evt.newValue;
            if (IsValueValid(evt.newValue)) {
                updateValue = evt.newValue;
            }
            else if (IsValueValid(evt.previousValue)) {
                updateValue = evt.previousValue;
            }
            else {
                updateValue = FromValue(AdjustValue(evt.newValue, evt.previousValue));
            }
        }
        protected override bool IsValueValid(TValue value) => IsValueValid(FromValue(value));
        protected virtual bool IsValueValid(string value) => maxLength <= 0 || value.Length <= maxLength;

        protected override TValue AdjustValue(string targetValue, string previousValue) {
            if (ToValue(AdjustStringValue(targetValue), out TValue r)) {
                return r;
            }
            else if (IsValueValid(previousValue) && ToValue(previousValue, out TValue s)) {
                return s;
            }
            else if (ToValue(AdjustStringValue(previousValue), out TValue t)) {
                return t;
            }
            else {
                return GetDefaultValue();
            }
        }

        protected override TValue AdjustValue(TValue value) {
            if (ToValue(AdjustStringValue(FromValue(value)), out TValue t))
                return t;
            else
                return GetDefaultValue();
        }
        protected virtual string AdjustStringValue(string targetValue, string previousValue) => AdjustStringValue(targetValue);

        protected virtual string AdjustStringValue(string value) {
            if (maxLength <= 0 || value.Length <= maxLength)
                return value;
            return value.Substring(0, maxLength);

        }
        public override void SetValueWithoutNotify(TValue newValue) {
            if (IsValueValid(newValue)) {
                _value = newValue;
            }
            else {
                var t = AdjustValue(newValue);
                if (IsValueValid(t)) {
                    _value = t;
                }
                else {
                    _value = GetDefaultValue();
                }
            }
            stringValue = FromValue(_value);
            updateValue = stringValue;
            textField.SetValueWithoutNotify(stringValue);
        }

    }
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CustomVisualElementProvider : Attribute {
        public CustomVisualElementProvider(Type type) {
            Type = type;
        }
        public Type Type { get; private set; }
    }



}