using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph
{
    public class ValueNode : Node
    {
        public VisualElement content { get; private set; }

        public Port port { get; private set; }

        public ValueNode(Type type) : base()
        {

            content = new VisualElement
            {
                name = "contents"
            };


            port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, type);
            port.portName = "";
            inputContainer.Add(content);


            content.Add(new Label("moose"));
            outputContainer.Add(port);

            this.style.flexDirection = FlexDirection.Row;
            this.style.flexGrow = 0;
            this.style.flexShrink = 1;
            this.style.backgroundColor = Color.gray;
            this.RefreshPorts();
            this.expanded = true;
            this.RefreshExpandedState();
        }

    }
}