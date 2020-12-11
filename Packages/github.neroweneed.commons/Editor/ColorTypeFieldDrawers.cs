using System;
using NeroWeNeed.Commons.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
[assembly: ValueTypeFieldDrawer]


namespace NeroWeNeed.Commons.Editor {
    [ValueTypeFieldDrawer(typeof(Color))]
    [ValueTypeFieldDrawer(typeof(Color32))]
    public sealed class ColorTypedFieldDrawer : ValueTypeFieldDrawer {
        public override VisualElement CreateElement(Type type, object initial) {
            var colorField = new ColorField();

            if (initial.GetType() == typeof(Color)) {
                colorField.value = (Color)initial;
            }
            else if (initial.GetType() == typeof(Color32)) {

                colorField.value = (Color32)initial;
                colorField.RegisterValueChangedCallback(evt =>
                {

                    using ChangeEvent<Color32> evt2 = ChangeEvent<Color32>.GetPooled(evt.previousValue, evt.newValue);
                    evt.currentTarget.SendEvent(evt2);
                });
            }
            return colorField;

        }
    }
}