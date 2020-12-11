using System;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons {
    public interface ITypedDrawerFieldHint {
        void Configure(VisualElement target);
    }
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public abstract class TypedDrawerFieldHintAttribute : Attribute, ITypedDrawerFieldHint {
        public abstract void Configure(VisualElement target);
    }

}