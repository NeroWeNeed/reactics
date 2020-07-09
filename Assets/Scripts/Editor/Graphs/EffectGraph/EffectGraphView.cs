using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Reactics.Battle;
using Reactics.Battle.Map;

namespace Reactics.Editor.Graph
{
    public class EffectGraphView : GraphView
    {
        private EffectGraphSearchWindow searchWindow;
        private EffectGraphModule module;
        private Func<Vector2, Vector2> screenToWorldConverter;

        public const string EffectGraphMasterNodeClassName = "master-node";
        private Node masterNode;
        public EffectGraphView(Func<Vector2, Vector2> screenToWorldConverter)
        {
            this.userData = new EffectGraphModelEditor();
            module = new EffectGraphModule();
            masterNode = CreateMasterNode(module);
            this.screenToWorldConverter = screenToWorldConverter;
            //styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Editor/UIElements/EffectGraph.uss"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new RectangleSelector());
            style.backgroundColor = new Color(0.109f, 0.109f, 0.109f);
            //this.AddManipulator(new ContextualMenuManipulator((evt) => BuildNodeMenu(evt.menu, contentViewContainer.WorldToLocal(contentViewContainer.parent.ChangeCoordinatesTo(contentViewContainer.parent, evt.mousePosition)))));
            this.contentViewContainer.RegisterCallback<GeometryChangedEvent>((evt) =>
            {
                if (evt.oldRect.width == 0 && evt.oldRect.height == 0)
                    FrameAll();

            });

            /*             this.RegisterCallback<MouseMoveEvent>((evt) =>
                        {
                            mousePosition = contentViewContainer.WorldToLocal(contentViewContainer.parent.ChangeCoordinatesTo(contentViewContainer.parent, evt.mousePosition));
                        }); */
            /*             serializeGraphElements = ToJson;
                        unserializeAndPaste = FromJson; */
            SetupSearchWindow();
            //SetupUndo();
            /*             serializeGraphElements = SerializeElements;
                        unserializeAndPaste = DeserializeElements; */

        }
        private Node CreateMasterNode(params IMasterNodeModule[] modules)
        {
            var masterNode = new Node
            {
                title = "Master"
            };
            foreach (var module in modules)
            {
                module.ConfigureMaster(masterNode);
            }
            masterNode.AddToClassList(EffectGraphMasterNodeClassName);
            masterNode.capabilities ^= Capabilities.Deletable;
            masterNode.RefreshPorts();
            this.AddElement(masterNode);
            return masterNode;
        }
        public void SetupSearchWindow()
        {
            searchWindow = ScriptableObject.CreateInstance<EffectGraphSearchWindow>();
            searchWindow.effectGraph = module;

            nodeCreationRequest = (context) =>
            {

                searchWindow.graphView = this;

                searchWindow.screenToWorldConverter = screenToWorldConverter;
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {

            var ports = new List<Port>();
            this.ports.ForEach((port) =>
            {

                if ((port.portType == typeof(IEffect) && !port.direction.Equals(startPort.direction)) || EqualityComparer<Type>.Default.Equals(port.portType, startPort.portType))
                    ports.Add(port);
            });
            return ports;
        }


    }
}