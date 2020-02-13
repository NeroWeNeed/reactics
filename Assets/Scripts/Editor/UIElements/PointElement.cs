using System;
using System.Collections.Generic;
using Reactics.Battle;
using Reactics.Debugger;
using Reactics.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Reactics.UIElements
{

    public class PointElement : BindableElement, INotifyValueChanged<Point>
    {
        private const string uxmlTemplate = "Assets/Editor/Point.uxml";
        private const string styleSheet = "Assets/Editor/Point.uss";

        private static readonly List<Action<PointElement>> selectorListener = new List<Action<PointElement>>();
        private MapDebugger debugger;

        private bool selecting = false;

        private IntegerField xField, yField;

        public PointElement(Point initial) : base()
        {
            Initialize();
            SetValueWithoutNotify(initial);
        }
        public PointElement() : base()
        {
            Initialize();
        }
        private void Initialize()
        {
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlTemplate).CloneTree(this);
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheet));
            xField = this.Q<IntegerField>("x");
            yField = this.Q<IntegerField>("y");
            EditorApplication.hierarchyChanged += ListenForDebugger;
            selectorListener.Add(UpdateSelectability);
            ListenForDebugger();
            xField.RegisterValueChangedCallback(x =>
            {
                if (value.x != x.newValue)
                {
                    value = new Point(x.newValue, value.y);
                }
            });
            yField.RegisterValueChangedCallback(x =>
            {
                if (value.x != x.newValue)
                {
                    value = new Point(value.x, x.newValue);
                }
            });
        }
        ~PointElement()
        {
            selectorListener.Remove(UpdateSelectability);
            EditorApplication.hierarchyChanged -= ListenForDebugger;
        }
        [SerializeField]
        private Point _value;
        public Point value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!EqualityComparer<Point>.Default.Equals(_value, value))
                {
                    if (panel != null)
                    {
                        using (ChangeEvent<Point> evt = ChangeEvent<Point>.GetPooled(_value, value))
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

        private void ListenForDebugger()
        {
            this.debugger = null;
            SceneView.duringSceneGui -= SelectPoint;
            foreach (var item in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (item.TryGetComponent(out MapDebugger debugger))
                {
                    this.debugger = debugger;
                    SceneView.duringSceneGui += SelectPoint;
                    this.Q<Button>("select").Apply(select =>
                    {
                        select.SetEnabled(true);
                        select.clicked += StartSelectPoint;

                    });

                    return;
                }
            }
            this.Q<Button>("select").Apply(select =>
                    {
                        select.SetEnabled(false);
                        select.clicked -= StartSelectPoint;
                    });
        }
        private void StartSelectPoint()
        {
            selectorListener.ForEach(x => x.Invoke(this));
        }
        private void SelectPoint(SceneView sceneView)
        {
            if (selecting)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Event.current.type == EventType.MouseDown && Physics.Raycast(ray, out RaycastHit hit) && debugger.MapRenderer.GetPoint(hit.point, out Point pointInfo))
                {
                    value = pointInfo;
                    selectorListener.ForEach(x => x.Invoke(null));
                }
            }
        }
        private void UpdateSelectability(PointElement element)
        {
            if (element == null)
            {
                selecting = false;
                this.Q<Button>("select").SetEnabled(true);
            }
            else if (element == this)
            {
                selecting = true;
                this.Q<Button>("select").SetEnabled(true);
            }
            else
            {
                selecting = false;
                this.Q<Button>("select").SetEnabled(false);
            }


        }

        public void SetValueWithoutNotify(Point newValue)
        {
            _value = newValue;
            xField.SetValueWithoutNotify(newValue.x);
            yField.SetValueWithoutNotify(newValue.y);
            MarkDirtyRepaint();
        }

        public new class UxmlFactory : UxmlFactory<PointElement, UxmlTraits> { }
        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlIntAttributeDescription x = new UxmlIntAttributeDescription { name = "x" };
            UxmlIntAttributeDescription y = new UxmlIntAttributeDescription { name = "y" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                PointElement pointElement = ve as PointElement;
                int x = this.x.GetValueFromBag(bag, cc);
                int y = this.y.GetValueFromBag(bag, cc);
                pointElement.value = x == 0 && y == 0 ? Point.zero : new Point(x, y);
            }


        }


    }

}