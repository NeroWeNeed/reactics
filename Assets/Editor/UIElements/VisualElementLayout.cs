using System;
using Reactics.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public abstract class VisualElementLayout : Attribute {
        public abstract void Layout(VisualElement element, VisualElement target);
    }
    public sealed class EmbeddedInputLayout : VisualElementLayout {
        public override void Layout(VisualElement element, VisualElement target) {
            if (target is Node node) {
                node.inputContainer.Add(element);
            }
            else {
                target.Add(element);
            }
        }
    }
    public sealed class EmbeddedOutputLayout : VisualElementLayout {
        public override void Layout(VisualElement element, VisualElement target) {
            if (target is Node node) {
                node.outputContainer.Add(element);
            }
            else {
                target.Add(element);
            }
        }
    }
    public abstract class NodeContainerLayout : VisualElementLayout {
        public abstract VisualElement GetContainer(Node node);
        public abstract Direction PortDirection { get; }
        public override void Layout(VisualElement element, VisualElement target) {
            if (target is Node node) {

                VisualElement valuePort;
                if (element is VisualElementDrawer drawer) {
                    valuePort = new ObjectGraphValuePort(drawer, PortDirection);
                }
                else {
                    valuePort = ValuePortElement.Create(element, node, PortDirection);
                }
                GetContainer(node).Add(valuePort);
                //valuePort.Attach();
            }
            else {
                target.Add(element);
            }
        }
    }
    public sealed class InputLayout : NodeContainerLayout {
        public override Direction PortDirection => Direction.Input;

        public override VisualElement GetContainer(Node node) => node.inputContainer;
    }
    public sealed class OutputLayout : NodeContainerLayout {
        public override Direction PortDirection => Direction.Output;

        public override VisualElement GetContainer(Node node) => node.inputContainer;
    }
}