using System;
using NeroWeNeed.Commons.Editor;
using UnityEngine.UIElements;

[assembly: ValueTypeFieldDrawer]
namespace NeroWeNeed.UIDots.Editor {

    [ValueTypeFieldDrawer(typeof(UILength))]
    public sealed class UILengthTypedFieldDrawer : ValueTypeFieldDrawer {

        public override VisualElement CreateElement(Type type, object initial) {
            return new UILengthField(null, (UILength)initial);
        }
    }

}
