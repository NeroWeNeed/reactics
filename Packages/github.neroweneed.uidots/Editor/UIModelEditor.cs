using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.UIDots.Editor {
    //[CustomEditor(typeof(UIModel))]
    public class UIModelEditor : UnityEditor.Editor {
        private VisualElement rootVisualElement;
        public override VisualElement CreateInspectorGUI() {
            if (rootVisualElement == null) {
                rootVisualElement = new VisualElement();
            }
            else {
                rootVisualElement.Clear();
            }
            var textfield = new TextField
            {
                multiline = true,
                value = (serializedObject.targetObject as TextAsset)?.text ?? "",
            };
            textfield.SetEnabled(false);
            rootVisualElement.Add(textfield);
            return rootVisualElement;
        }
    }
}