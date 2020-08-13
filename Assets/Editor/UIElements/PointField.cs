using System;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Reactics.Core.Editor {
    public class PointField : BindableElement, INotifyValueChanged<Point> {
        private Label labelElement;

        private VisualElement fields;

        private VisualElement spacer;

        private Point _value;

        public string label { get => labelElement.text; set => labelElement.text = value; }
        public Point value
        {
            get => _value; set
            {
                if (value.Equals(_value)) {
                    if (panel != null) {

                        using (ChangeEvent<Point> evt = ChangeEvent<Point>.GetPooled(_value, value)) {
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

        public void SetValueWithoutNotify(Point newValue) {
            _value = newValue;
        }
        public PointField() : this("Point", Point.zero) {

        }
        public PointField(string label, Point value) {
            this.labelElement = new Label(label);
            this.labelElement.style.minWidth = 150;
            this.labelElement.style.paddingLeft = 1;
            this.labelElement.style.paddingTop = 2;
            this.labelElement.style.paddingRight = 2;
            this.style.marginLeft = 3;
            this.style.marginRight = 3;
            this.style.marginTop = 1;
            this.style.marginBottom = 1;
            fields = new VisualElement();
            spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            var xField = new IntegerField("X", 5)
            {
                name = "point-field-x"
            };
            xField.AddToClassList("unity-composite-field__field--first");

            var yField = new IntegerField("Y", 5)
            {
                name = "point-field-y"
            };
            ConfigureField(xField, (x, point) => new Point(x, point.y));
            ConfigureField(yField, (y, point) => new Point(point.x, y));
            fields.Add(xField);
            fields.Add(yField);
            fields.Add(spacer);
            Add(this.labelElement);
            Add(fields);

            SetValueWithoutNotify(value);
            style.flexDirection = FlexDirection.Row;
            style.flexShrink = 0;
            style.flexGrow = 1;
            fields.style.flexDirection = FlexDirection.Row;
            fields.style.flexGrow = 1;
            fields.style.flexShrink = 0;
        }
        public PointField(string label) : this(label, Point.zero) { }
        public PointField(Point value) : this("Point", value) { }
        private void ConfigureField(IntegerField field, Func<ushort, Point, Point> writer) {
            field.style.minWidth = 0;
            field.style.flexGrow = 1;
            field.style.flexDirection = FlexDirection.Row;
            field.AddToClassList("unity-composite-field__field");
            field.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue >= 0 && evt.newValue <= ushort.MaxValue) {
                    writer((ushort)evt.newValue, value);
                }
                else {
                    if (evt.newValue > ushort.MaxValue)
                        field.SetValueWithoutNotify(ushort.MaxValue);
                    else if (evt.newValue < 0)
                        field.SetValueWithoutNotify(0);
                }
                evt.StopImmediatePropagation();
            });
        }

        public new class UxmlFactory : UxmlFactory<PointField, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits {
            UxmlIntAttributeDescription m_XValue = new UxmlIntAttributeDescription { name = "x" };
            UxmlIntAttributeDescription m_YValue = new UxmlIntAttributeDescription { name = "y" };
            UxmlStringAttributeDescription m_Label = new UxmlStringAttributeDescription { name = "label", defaultValue = "Point" };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var f = (PointField)ve;
                var x = m_XValue.GetValueFromBag(bag, cc);
                var y = m_YValue.GetValueFromBag(bag, cc);
                x = x >= 0 && x <= ushort.MaxValue ? x : 0;
                y = y >= 0 && y <= ushort.MaxValue ? y : 0;
                f.labelElement.text = m_Label.GetValueFromBag(bag, cc);
                f.SetValueWithoutNotify(new Point(x, y));
            }
        }
    }
}