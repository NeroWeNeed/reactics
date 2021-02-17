using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.UIDots.Editor {
    public class UILengthField : BindableElement, INotifyValueChanged<UILength> {
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
        public UILengthField(string label = null, UILength initialValue = default) : base() {
            Init(label, initialValue);
        }

        public UILengthField() {
            Init(null, default);
        }

        private void Init(string label, UILength initialValue) {
            this.AddToClassList("unity-base-field");
            this.AddToClassList("unity-base-text-field");
            labelElement = new Label(label);
            labelElement.AddToClassList("unity-base-field__label");
            labelElement.AddToClassList("unity-property-field__label");
            valueField = new FloatField
            {
                value = initialValue.value,
                bindingPath = "value"
            };
            valueField.RegisterValueChangedCallback(evt => this.value = new UILength(evt.newValue, this.value.unit));
            
            unitField = new EnumField(initialValue.unit)
            {
                bindingPath = "unit"
            };
            unitField.RegisterValueChangedCallback(evt =>
            {
                this.value = new UILength(this.value.value, evt.newValue == null ? default : (UILengthUnit)evt.newValue);
            });
            this.style.flexDirection = FlexDirection.Row;
            valueField.style.flexGrow = 1;
            valueField.style.marginLeft = 0;
            unitField.style.minWidth = 80;
            unitField.style.marginLeft = 0;
            unitField.style.marginRight = 0;
            this.Add(labelElement);
            this.Add(valueField);
            this.Add(unitField);
        }

        public void SetValueWithoutNotify(UILength newValue) {
            rawValue = newValue;
            valueField.SetValueWithoutNotify(newValue.value);
            unitField.SetValueWithoutNotify(newValue.unit);
        }
        public new class UxmlFactory : UxmlFactory<UILengthField, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits {
            UxmlStringAttributeDescription value = new UxmlStringAttributeDescription { name = "value" };
            UxmlStringAttributeDescription label = new UxmlStringAttributeDescription { name = "label", defaultValue = "Length" };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var f = (UILengthField)ve;
                f.labelElement.text = label.GetValueFromBag(bag, cc);
                if (UILength.TryParse(this.value.GetValueFromBag(bag, cc), out UILength result)) {
                    f.SetValueWithoutNotify(result);
                }
            }
        }
    }
}