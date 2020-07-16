using System;
using Reactics.Commons;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace Reactics.Editor {

    [CustomVisualElementProvider(typeof(sbyte))]
    public class SignedByteDrawer : NumericTextValueDrawer<sbyte> { public SignedByteDrawer() : base((string value, out sbyte result) => sbyte.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(byte))]
    public class ByteDrawer : NumericTextValueDrawer<byte> { public ByteDrawer() : base((string value, out byte result) => byte.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(short))]
    public class ShortDrawer : NumericTextValueDrawer<short> { public ShortDrawer() : base((string value, out short result) => short.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(ushort))]
    public class UnsignedShortDrawer : NumericTextValueDrawer<ushort> { public UnsignedShortDrawer() : base((string value, out ushort result) => ushort.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(int))]
    public class IntDrawer : NumericTextValueDrawer<int> { public IntDrawer() : base((string value, out int result) => int.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(uint))]
    public class UnsignedIntDrawer : NumericTextValueDrawer<uint> { public UnsignedIntDrawer() : base((string value, out uint result) => uint.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(long))]
    public class LongDrawer : NumericTextValueDrawer<long> { public LongDrawer() : base((string value, out long result) => long.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(ulong))]
    public class UnsignedLongDrawer : NumericTextValueDrawer<ulong> { public UnsignedLongDrawer() : base((string value, out ulong result) => ulong.TryParse(value, IntegerPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(float))]
    public class FloatDrawer : NumericTextValueDrawer<float> { public FloatDrawer() : base((string value, out float result) => float.TryParse(value, FloatingPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(double))]
    public class DoubleDrawer : NumericTextValueDrawer<double> { public DoubleDrawer() : base((string value, out double result) => double.TryParse(value, FloatingPointStyle, System.Globalization.CultureInfo.InvariantCulture, out result)) { } }
    [CustomVisualElementProvider(typeof(char))]
    public class CharDrawer : NumericTextValueDrawer<char> {
        public CharDrawer() : base((string value, out char result) => char.TryParse(value, out result)) { }
        public override void Initialize(string label = null, char initial = default, Attribute[] attributes = null) {
            base.Initialize(label, initial);
            textField.maxLength = 1;
            var textInput = textField.Q<VisualElement>("unity-text-input");
            textInput.style.minWidth = textInput.style.maxWidth = 11;
        }
    }
    [CustomVisualElementProvider(typeof(bool))]
    [InputLayout]
    public class BoolDrawer : VisualElementDrawer<bool> {
        public bool value
        {
            get => _value; set
            {

                if (!value.Equals(_value)) {

                    if (panel != null) {
                        using (ChangeEvent<bool> evt = ChangeEvent<bool>.GetPooled(_value, value)) {
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
        public Toggle toggle { get; protected set; }
        public override string Label { get => toggle.label; set => toggle.label = value; }
        public override void Initialize(string label, bool initialValue, Attribute[] attributes = null) {

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

        public override void SetValueWithoutNotify(bool newValue) {
            _value = newValue;
            toggle.SetValueWithoutNotify(newValue);
        }
    }
    [CustomVisualElementProvider(typeof(Enum))]
    [InputLayout]
    public class EnumDrawer : VisualElementDrawer<Enum> {
        public EnumField enumField { get; protected set; }

        public override string Label { get => enumField.label; set => enumField.label = value; }
        public override void Initialize(string label, Enum initialValue, Attribute[] attributes = null) {
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

        public override void SetValueWithoutNotify(Enum newValue) {
            _value = newValue;
            enumField.SetValueWithoutNotify(newValue);
        }
    }
    [CustomVisualElementProvider(typeof(BlittableAssetReference64))]
    [InputLayout]
    public class AssetReferenceDrawer : VisualElementDrawer<BlittableAssetReference64> {
        private AssetReferenceSearchField searchField;

        public override string Label { get => searchField.label; set => searchField.label = value; }
        public override void Initialize(string label, BlittableAssetReference64 initialValue, Attribute[] attributes = null) {
            searchField = new AssetReferenceSearchField(label);
            this.SetValueWithoutNotify(initialValue);
            searchField.RegisterValueChangedCallback(OnValueInSearchFieldChanged);
            this.Add(searchField);
        }
        private void OnValueInSearchFieldChanged(ChangeEvent<AssetReference> evt) {
            if (evt.newValue != null && !string.IsNullOrEmpty(evt.newValue.SubObjectName) && evt.newValue.SubObjectName.Length > BlittableAssetReference64.SubObjectNameMaxLength) {
                Debug.LogError($"Selected SubObject of Asset must have a name less than or equal to {BlittableAssetReference64.SubObjectNameMaxLength}");
                return;
            }
            value = (BlittableAssetReference64)evt.newValue;
        }

    }
    [CustomVisualElementProvider(typeof(string))]
    [InputLayout]
    public class StringDrawer : TextValueDrawer<string> {
        public StringDrawer() : base((string value, out string result) =>
        {
            result = value;
            return true;
        }) {
        }
    }





}