using Reactics.Core.Commons;
using Reactics.Core.Map;
using UnityEditor;
using UnityEngine;
namespace Reactics.Core.Editor {


    [CustomPropertyDrawer(typeof(Point))]
    public class PointPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var xLabelRect = new Rect(position.x, position.y, position.width * 0.1f, position.height);
            var xFieldRect = new Rect(position.x + position.width * 0.1f, position.y, position.width * 0.3f, position.height);

            var yLabelRect = new Rect(position.x + position.width * 0.4f, position.y, position.width * 0.1f, position.height);
            var yFieldRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.3f, position.height);
            var selectButtonRect = new Rect(position.x + position.width * 0.8f, position.y, position.width * 0.2f, position.height);

            var xProperty = property.FindPropertyRelative("x");
            var yProperty = property.FindPropertyRelative("y");
            EditorGUI.LabelField(xLabelRect, "X");
            var x = EditorGUI.IntField(xFieldRect, xProperty.intValue);
            EditorGUI.LabelField(yLabelRect, "Y");
            var y = EditorGUI.IntField(yFieldRect, yProperty.intValue);
            if (x != xProperty.intValue && x >= 0 && x < ushort.MaxValue)
                xProperty.intValue = x;
            if (y != yProperty.intValue && y >= 0 && y < ushort.MaxValue)
                yProperty.intValue = y;
            EditorGUI.EndProperty();
        }
    }
}