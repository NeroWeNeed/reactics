using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactics.Battle;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace Reactics.Editor
{
    public class EffectGraphView : GraphView
    {
        private static List<Type> validTypes;
        public static ReadOnlyCollection<Type> ValidTypes { get; private set; }

        static EffectGraphView()
        {
            PopulateTypeList();
        }
        private Vector2 mousePosition = new Vector2();
        private EffectGraph effectGraph;
        private EffectGraphSearchWindow searchWindow;

        private Func<Vector2, Vector2> screenToWorldConverter;
        public EffectGraphView(EffectGraph effectGraph, Func<Vector2, Vector2> screenToWorldConverter)
        {
            this.effectGraph = effectGraph;
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

            this.RegisterCallback<MouseMoveEvent>((evt) =>
            {
                mousePosition = contentViewContainer.WorldToLocal(contentViewContainer.parent.ChangeCoordinatesTo(contentViewContainer.parent, evt.mousePosition));
            });
            serializeGraphElements = ToJson;
            unserializeAndPaste = FromJson;
            SetupSearchWindow();
            SetupUndo();
            /*             serializeGraphElements = SerializeElements;
                        unserializeAndPaste = DeserializeElements; */

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
        public void SetupSearchWindow()
        {
            searchWindow = ScriptableObject.CreateInstance<EffectGraphSearchWindow>();
            searchWindow.effectGraph = effectGraph;

            nodeCreationRequest = (context) =>
            {

                searchWindow.graphView = this;
                searchWindow.effectGraph = effectGraph;
                searchWindow.screenToWorldConverter = screenToWorldConverter;
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
            };
        }

        public void SetupUndo()
        {
/*             Undo.undoRedoPerformed += () =>
            {
                
                EffectGraphController.RefreshValues(effectGraph, this, (port, graphView) =>
                {
                    var master = graphView.Q<EffectMasterNode>(null, EffectGraphController.EffectGraphMasterNodeClassName);
                    if (master != null)
                        graphView.AddElement(master.EffectPort.ConnectTo(port));
                });
            }; */
        }

        private static void PopulateTypeList()
        {
            if (validTypes == null)
                validTypes = new List<Type>();
            else
                validTypes.Clear();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    if (IsValidType(type))
                        validTypes.Add(type);
            ValidTypes = validTypes.AsReadOnly();

        }
        public static bool IsValidType(Type type)
        {
            return type.IsUnmanaged() && typeof(IEffect).IsAssignableFrom(type) && !typeof(IUtilityEffect).IsAssignableFrom(type);
        }
        private string ToJson(IEnumerable<GraphElement> elements)
        {
            var nodeIds = new List<Guid>();
            var nodeLayouts = new List<Rect>();
            foreach (var element in elements)
                if (element.ClassListContains(EffectGraphController.EffectGraphNodeClassName) && Guid.TryParse(element.viewDataKey, out Guid id))
                {
                    nodeIds.Add(id);
                    nodeLayouts.Add(element.GetPosition());
                }
            var r = JsonConvert.SerializeObject(new JsonData
            {
                effects = nodeIds.Select((id) => effectGraph.entries[id]).ToArray(),
                layouts = nodeLayouts.Select((rect, index) => new JsonData.LayoutData
                {
                    id = nodeIds[index],
                    layout = rect
                }).ToArray()
            }, new EffectGraphEntryConverter(), new RectConverter());
            return r;

        }
        private void FromJson(string operationName, string data)
        {
            var jsonData = JsonConvert.DeserializeObject<JsonData>(data, new EffectGraphEntryConverter(), new RectConverter());

            EffectGraphController.CopyEntries(jsonData.effects, out EffectGraph.Entry[] newEntries, out Guid[] oldIds);
            var layouts = new Rect[newEntries.Length];
            Vector2 topLeft = new Vector2(jsonData.layouts[0].layout.xMin, jsonData.layouts[0].layout.yMin);
            for (int i = 0; i < jsonData.layouts.Length; i++)
            {
                topLeft.x = topLeft.x > jsonData.layouts[i].layout.xMin ? jsonData.layouts[i].layout.xMin : topLeft.x;
                topLeft.y = topLeft.y > jsonData.layouts[i].layout.yMin ? jsonData.layouts[i].layout.yMin : topLeft.y;
            }

            for (int i = 0; i < newEntries.Length; i++)
            {

                var layout = new Rect(jsonData.layouts.First((x) => x.id.Equals(oldIds[i])).layout);
                layout.xMin = layout.xMin - topLeft.x + mousePosition.x;
                layout.xMax = layout.xMax - topLeft.x + mousePosition.x;
                layout.yMin = layout.yMin - topLeft.y + mousePosition.y;
                layout.yMax = layout.yMax - topLeft.y + mousePosition.y;
                layouts[i] = layout;

            }
            EffectGraphController.AdjustConnections(newEntries, oldIds);
            EffectGraphController.CreateNodes(this, effectGraph, newEntries, layouts);


        }
        public struct JsonData
        {
            public EffectGraph.Entry[] effects;
            public LayoutData[] layouts;
            public struct LayoutData
            {
                public Guid id;

                public Rect layout;
            }
        }

    }
}