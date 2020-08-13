using System;
using Reactics.Core.UI;
using UnityEditor;
using UnityEngine;
namespace Reactics.Core.Editor.Drawers {


    //TODO: UI Toolkit Drawer Implementation (Low Priority)
    [CustomPropertyDrawer(typeof(UILength))]
    public class UILengthDrawer : PropertyDrawer {
        private readonly Array values = Enum.GetValues(typeof(UILengthUnit));
        private const float UNIT_POPUP_SIZE = 60f;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            var valueRect = new Rect(position.x, position.y, position.width - UNIT_POPUP_SIZE, position.height);
            var unitRect = new Rect(position.x + position.width - UNIT_POPUP_SIZE, position.y, UNIT_POPUP_SIZE, position.height);

            var valueProperty = property.FindPropertyRelative("value");
            var unitProperty = property.FindPropertyRelative("unit");
            float value = EditorGUI.FloatField(valueRect, valueProperty.floatValue);
            int unitIndex = Array.IndexOf(values, EditorGUI.EnumPopup(unitRect, (UILengthUnit)values.GetValue(unitProperty.enumValueIndex)));
            if (value != valueProperty.floatValue) {
                valueProperty.floatValue = value;
            }
            if (unitIndex != unitProperty.enumValueIndex) {
                unitProperty.enumValueIndex = unitIndex;
            }

            EditorGUI.EndProperty();
        }
    }
}