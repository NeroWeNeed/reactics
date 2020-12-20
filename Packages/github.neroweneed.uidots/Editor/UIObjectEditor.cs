using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
namespace NeroWeNeed.UIDots.Editor {

    [CustomEditor(typeof(UIObject))]
    public class UIObjectEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
        }
        public bool HasFrameBounds() {
            return (target as UIObject)?.CachedMesh != null;
        }

        public Bounds OnGetFrameBounds() {
            var transform = Selection.activeTransform;
            
            var bo = (target as UIObject)?.Bounds ?? default;
            return new Bounds(float3.zero,bo.size);
        }
    }

}