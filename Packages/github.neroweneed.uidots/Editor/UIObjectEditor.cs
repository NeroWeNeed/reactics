using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NeroWeNeed.UIDots.Editor {

    [CustomEditor(typeof(UIObject))]
    public class UIObjectEditor : UnityEditor.Editor {
        public UIObject UIObject { get => target as UIObject; }
        private const string UXML = "Packages/github.neroweneed.uidots/Editor/Resources/UIObject.uxml";
        public override VisualElement CreateInspectorGUI() {
            var obj = UIObject;
            if (obj != null) {
                obj.transform.hideFlags = HideFlags.HideInInspector;
            }
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML);
            var inspector = uxml.CloneTree();
            inspector.Q<PropertyField>("camera").RegisterValueChangeCallback((evt) => inspector.Query<VisualElement>(null, "require-camera").ForEach((y) => y.SetEnabled(evt.changedProperty.objectReferenceValue != null)));
            inspector.Bind(serializedObject);
            return inspector;
        }
        public bool HasFrameBounds() {
            return UIObject?.cachedMesh != null;
        }
        public Bounds OnGetFrameBounds() {

            return UIObject?.Bounds ?? default;
        }
    }

}