using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {
    public class ActionGraphModule : IObjectGraphModule, IInspectorConfigurator {
        public VisualElement CreateInspectorSection(SerializedObject obj, ObjectGraphView graphView) {
            var container = new BindableElement();

            var info = obj.FindProperty("info");
            Debug.Log(info);
            if (info != null)
                container.Add(new PropertyField(info));
            container.Bind(obj);
            return container;
        }
    }
}