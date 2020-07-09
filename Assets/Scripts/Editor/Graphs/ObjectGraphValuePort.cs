using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class StandaloneField : Attribute { }


    [StandaloneField]
    public class ObjectGraphValuePort<TValue> : VisualElementDrawer<TValue>, INotifyValueChanged<TValue>
    {
        private ValueNode pill;
        private Attacher attacher = null;
        private Port port;
        public ObjectGraphValuePort()
        {
            this.port = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(ObjectGraphValuePort<TValue>));
            pill = new ValueNode(typeof(ObjectGraphValuePort<TValue>));
            this.Add(port);

            this.RegisterCallback<AttachToPanelEvent>(AddPill);
            this.RegisterCallback<DetachFromPanelEvent>(RemovePill);

        }
        public void SetPillElement<TElement>(TElement element) where TElement : VisualElement, INotifyValueChanged<TValue>
        {
            pill.content.Clear();
            pill.content.Add(element);
            element.RegisterValueChangedCallback((evt) =>
            {
                this.value = evt.newValue;
            });
        }
        private void AddPill(AttachToPanelEvent evt)
        {
            var graphView = this.GetFirstAncestorOfType<GraphView>();
            if (graphView == null)
                return;
            graphView.AddElement(pill);
            graphView.AddElement(pill.port.ConnectTo(port));
            if (attacher == null)
            {
                attacher = new Attacher(pill, this, SpriteAlignment.LeftCenter);

            }
            else
            {
                attacher.Reattach();
            }
        }
        private void RemovePill(DetachFromPanelEvent evt)
        {
            var graphView = this.GetFirstAncestorOfType<GraphView>();
            if (graphView == null)
                return;
            graphView.Remove(pill);
            pill.port.DisconnectAll();
            if (attacher != null)
            {
                attacher.Detach();

            }
        }
        private TValue _value;
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

        public void SetValueWithoutNotify(TValue newValue)
        {

            _value = newValue;
            (pill.content as INotifyValueChanged<TValue>).SetValueWithoutNotify(newValue);
        }

        public override void Initialize(string label, TValue initialValue, Attribute[] attributes = null)
        {
            port.portName = label;
            if (pill.content is VisualElementDrawer drawer)
                drawer.Initialize(null, initialValue, attributes);

        }
    }
}