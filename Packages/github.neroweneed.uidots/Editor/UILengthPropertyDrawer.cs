using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.UIDots.Editor {
    [CustomPropertyDrawer(typeof(UILength))]
    public sealed class UILengthPropertyDrawer : PropertyDrawer {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) {
            var container = new VisualElement();
            
            var field = new UILengthField(property.name);
            container.Add(field);
            return container;

        }


    }
}