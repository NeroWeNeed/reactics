using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace NeroWeNeed.UIDots.Editor {
    public class UILengthField : VisualElement, INotifyValueChanged<UILength> {
        public string label
        {
            get => labelElement?.text; set
            {
                labelElement.text = value;
            }
        }
        private Label labelElement;
        private FloatField valueField;
        private EnumField unitField;
        private UILength rawValue;
        public UILength value
        {
            get => rawValue; set
            {
                if (!EqualityComparer<UILength>.Default.Equals(value, rawValue)) {

                    if (panel != null) {
                        using var evt = ChangeEvent<UILength>.GetPooled(rawValue, value);
                        evt.target = this;
                        SetValueWithoutNotify(value);
                        SendEvent(evt);
                    }
                    else {
                        SetValueWithoutNotify(value);
                    }
                }
            }
        }
        public UILengthField(string label = null, UILength initialValue = default) {
            Init(label, initialValue);
        }
        private void Init(string label, UILength initialValue) {
            labelElement = new Label(label);
            valueField = new FloatField
            {
                value = initialValue.value
            };
            valueField.RegisterValueChangedCallback(evt => this.value = new UILength(evt.newValue, this.value.unit));
            unitField = new EnumField(initialValue.unit);
            unitField.RegisterValueChangedCallback(evt => this.value = new UILength(this.value.value, (UILengthUnit)evt.newValue));
            this.style.flexDirection = FlexDirection.Row;
            valueField.style.flexGrow = 1;
            unitField.style.minWidth = 50;
            this.Add(labelElement);
            this.Add(valueField);
            this.Add(unitField);
        }

        public void SetValueWithoutNotify(UILength newValue) {
            rawValue = newValue;
            valueField.SetValueWithoutNotify(newValue.value);
            unitField.SetValueWithoutNotify(newValue.unit);
        }
    }
}