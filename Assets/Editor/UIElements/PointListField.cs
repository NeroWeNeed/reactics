using System;
using System.Collections.Generic;
using Reactics.Core.Commons;
using Reactics.Core.Map;
using UnityEngine;
using UnityEngine.UIElements;
namespace Reactics.Core.Editor {
    public class PointList : BindableElement, INotifyValueChanged<Point[]> {
        private VisualElement container;
        private Point[] _value;
        public Point[] value
        {
            get => _value; set
            {
                if (value.Equals(_value)) {
                    if (panel != null) {
                        using (ChangeEvent<Point[]> evt = ChangeEvent<Point[]>.GetPooled(_value, value)) {
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
        private int _maxPoints;
        public int maxPoints
        {
            get => _maxPoints;
            set
            {
                var oldLength = this.value == null ? 0 : this.value.Length;
                if (value >= 0 && oldLength > value) {
                    var difference = value - oldLength;
                    var newPoints = new Point[maxPoints];
                    if (this.value != null)
                        Array.Copy(this.value, newPoints, maxPoints);
                    this._value = newPoints;
                    _maxPoints = value;
                    for (int i = 0; i < difference; i++) {
                        RemoveLast();
                    }
                }
                else
                    _maxPoints = value;
            }
        }
        private int _minPoints;
        public int minPoints
        {
            get => _minPoints;
            set
            {
                var oldLength = this._value == null ? 0 : this._value.Length;
                if (oldLength < value) {
                    var difference = value - oldLength;
                    var newPoints = new Point[value];
                    if (this.value != null)
                        Array.Copy(this._value, newPoints, oldLength);
                    this._value = newPoints;
                    _minPoints = value;
                    for (int i = 0; i < difference; i++) {
                        Add();
                    }
                }
                else
                    _minPoints = value;
            }
        }
        public int pointCount
        {
            get => _value.Length;
            set
            {
                Debug.Log("???");
                Debug.Log(_value.Length);
                Debug.Log(value);
                if (_value.Length == value)
                    return;
                if (_value.Length < 0)
                    throw new Exception("Point Count must be larger than or equal to 0");
                var newPoints = new Point[value];
                if (_value.Length > 0 && newPoints.Length > 0)
                    Array.Copy(_value, newPoints, value > _value.Length ? _value.Length : value);
                SetValueWithoutNotify(newPoints);

            }
        }
        public PointList() : this(-1, -1) {

        }
        public PointList(int min, int max, params Point[] points) {
            container = new VisualElement();
            _value = new Point[points.Length > min ? points.Length : min];
            maxPoints = max;
            minPoints = min;
            foreach (var point in points)
                Add(point);
            while (pointCount < min)
                Add();
            this.Add(container);
            UpdateElementButtons();
        }
        public void SetValueWithoutNotify(Point[] newValue) {
            _value = newValue;
        }
        private void SetValueAt(int index, Point value) {
            if (index < 0 || index >= this.value.Length || value.Equals(this.value[index]))
                return;

            if (panel != null) {
                using (ChangeEvent<Point[]> evt = ChangeEvent<Point[]>.GetPooled(_value, _value)) {
                    evt.target = this;
                    _value[index] = value;
                    SendEvent(evt);
                }
            }
            else {
                _value[index] = value;
            }

        }
        private void Add() => Add(-1, Point.zero);
        private void Add(Point point) => Add(-1, point);
        private void Add(int index) => Add(index, Point.zero);
        private void Add(int index, Point point) {
            if (maxPoints < 0 || pointCount + 1 < maxPoints) {
                var element = new PointListElement();

                element.addButton.clicked += () =>
                {
                    Add(container.IndexOf(element));
                };
                element.deleteButton.clicked += () =>
                {
                    Remove(container.IndexOf(element));
                };
                if (index < 0)
                    container.Add(element);
                else
                    container.Insert(index, element);
                UpdateValueArray(pointCount + 1);
                element.value = point;
                UpdateElementButtons();
            }
        }
        private void Remove(int index) {

            if (index < 0 || index >= container.childCount || container.childCount - 1 < minPoints)
                return;
            container.RemoveAt(index);

            UpdateValueArray(pointCount - 1);
            UpdateElementButtons();
        }
        private void RemoveLast() => Remove(container.childCount - 1);
        private void UpdateElementButtons() {
            Debug.Log("---");
            Debug.Log(minPoints);
            Debug.Log(pointCount);
            Debug.Log("---");
            var canAdd = pointCount + 1 < maxPoints || maxPoints < 0;
            var canDelete = pointCount - 1 > minPoints;
            for (int i = 0; i < container.childCount; i++) {
                (container[i] as PointListElement).deleteButton.SetEnabled(canDelete);
                (container[i] as PointListElement).addButton.SetEnabled(canAdd);
            }

        }
        private void UpdateValueArray(int newLength) {

            if (value == null || newLength != value.Length) {
                var newValues = new Point[newLength];
                if (value != null)
                    Array.Copy(value, newValues, newLength > value.Length ? value.Length : newLength);
                value = newValues;
            }
        }
        private class PointListElement : VisualElement, INotifyValueChanged<Point> {
            public static readonly string ussClassName = "reactics-point-list";
            public static readonly string addUssClassName = "__add-button";
            public static readonly string deleteUssClassName = "__delete-button";
            public static readonly string selectUssClassName = "__select-button";
            public static readonly string fieldUssClassName = "__field";
            public static readonly string addButtonUssClassName = ussClassName + addUssClassName;
            public static readonly string deleteButtonUssClassName = ussClassName + deleteUssClassName;
            public static readonly string selectButtonUssClassName = ussClassName + selectUssClassName;
            public static readonly string pointFieldUssClassName = ussClassName + fieldUssClassName;

            public readonly PointField pointField;
            public readonly Button addButton;
            public readonly Button deleteButton;
            public readonly Button selectButton;
            public PointListElement() : this(Point.zero) { }
            public PointListElement(Point point) {

                pointField = new PointField(point);
                pointField.AddToClassList(pointFieldUssClassName);
                pointField.RegisterValueChangedCallback((evt) => value = evt.newValue);
                addButton = new Button()
                {
                    text = "+"
                };
                addButton.AddToClassList(addButtonUssClassName);
                deleteButton = new Button()
                {
                    text = "-"
                };
                deleteButton.AddToClassList(deleteButtonUssClassName);
                selectButton = new Button()
                {
                    text = "⊙"
                };
                selectButton.AddToClassList(selectButtonUssClassName);
                Add(pointField);
                Add(deleteButton);
                Add(addButton);
                Add(selectButton);
                this.style.flexDirection = FlexDirection.Row;
            }
            public Point value
            {
                get => pointField.value; set
                {
                    if (panel != null) {
                        using (ChangeEvent<Point> evt = ChangeEvent<Point>.GetPooled(pointField.value, value)) {
                            evt.target = this;
                            SetValueWithoutNotify(value);
                            if (parent != null && parent.parent != null && parent.parent is PointList) {
                                (parent.parent as PointList).SetValueAt(parent.IndexOf(this), value);
                            }
                            SendEvent(evt);
                        }
                    }
                    else {
                        SetValueWithoutNotify(value);
                    }
                }

            }

            public void SetValueWithoutNotify(Point newValue) {
                pointField.SetValueWithoutNotify(newValue);
            }
        }
        public new class UxmlFactory : UxmlFactory<PointList, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits {

            UxmlIntAttributeDescription m_MaxPoints = new UxmlIntAttributeDescription { name = "max-points", defaultValue = -1 };
            UxmlIntAttributeDescription m_MinPoints = new UxmlIntAttributeDescription { name = "min-points", defaultValue = -1 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                var f = (PointList)ve;
                f.maxPoints = m_MaxPoints.GetValueFromBag(bag, cc);
                f.minPoints = m_MinPoints.GetValueFromBag(bag, cc);

            }
        }

    }
}
