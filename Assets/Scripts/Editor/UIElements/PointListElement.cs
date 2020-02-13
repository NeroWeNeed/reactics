using System;
using System.Collections.Generic;
using System.Linq;
using Reactics.Battle;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.UIElements {
    
    public class PointListElement : BindableElement, INotifyValueChanged<Point[]>
    {
        [SerializeField]
        private Point[] _value;
        public Point[] value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!EqualityComparer<Point[]>.Default.Equals(_value, value))
                {
                    if (panel != null)
                    {
                        using (ChangeEvent<Point[]> evt = ChangeEvent<Point[]>.GetPooled(_value, value))
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

        private Dictionary<string, Point> values = new Dictionary<string, Point>();
        private VisualElement children;
        private Button newPointButton;

        public PointListElement() : base()
        {
            Initialize();
        }

        public PointListElement(int size) : base()
        {
            Initialize();
            for (int i = 0; i < size; i++)
                AddPoint();
        }
        public PointListElement(params Point[] points) : base()
        {
            Initialize();
            foreach (var item in points)
            {
                AddPoint(item);
            }
        }
        private void Initialize()
        {
            children = new VisualElement
            {
                name = "points"
            };
            Add(children);
            newPointButton = new Button
            {
                text = "New Point",
                name = "new-point",
                visible = true
            };
            newPointButton.clicked += () =>
            {
                AddPoint();
            };
            Add(newPointButton);
        }
        public void AddPoint(int index = -1) => AddPoint(Point.zero, index);
        public void AddPoint(Point point, int index = -1)
        {
            PointElement pointElement = new PointElement(point);
            string guid = Guid.NewGuid().ToString();
            pointElement.name = guid;
            values[guid] = point;
            pointElement.RegisterCallback<ChangeEvent<Point>>(x =>
            {
                values[guid] = x.newValue;
                value = values.Values.ToArray();
            });
            Button addButton = new Button
            {
                name = "add",
                text = "+"
            };
            addButton.AddToClassList("ui-button");
            addButton.clicked += () =>
            {
                AddPoint(children.IndexOf(pointElement));
            };
            Button removeButton = new Button
            {
                name = "remove",
                text = "-"
            };
            removeButton.AddToClassList("ui-button");
            removeButton.clicked += () =>
            {
                pointElement.RemoveFromHierarchy();
                values.Remove(guid);
                newPointButton.style.display = children.childCount <= 0 ? DisplayStyle.Flex : DisplayStyle.None;
                value = values.Values.ToArray();
            };
            VisualElement root = pointElement.Q<VisualElement>("root");
            int buttonIndex = root.IndexOf(pointElement.Q<Button>("select"));
            root.Insert(buttonIndex, removeButton);
            root.Insert(buttonIndex, addButton);
            if (index < 0)
                children.Add(pointElement);
            else
                children.Insert(index + 1, pointElement);
            newPointButton.style.display = children.childCount <= 0 ? DisplayStyle.Flex : DisplayStyle.None;

            value = values.Values.ToArray();
        }
        public void SetValueWithoutNotify(Point[] newValue)
        {
            _value = newValue;
        }
        public new class UxmlFactory : UxmlFactory<PointListElement, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlIntAttributeDescription size = new UxmlIntAttributeDescription { name = "size" };
            UxmlBoolAttributeDescription modifiable = new UxmlBoolAttributeDescription { name = "modifiable", defaultValue = true };


            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                PointListElement pointListElement = ve as PointListElement;
                int size = this.size.GetValueFromBag(bag, cc);
                bool modifiable = this.modifiable.GetValueFromBag(bag, cc);
                for (int i = 0; i < size; i++)
                    pointListElement.AddPoint();
            }


        }
    }

}