using System;
using NeroWeNeed.Commons;
using UnityEditor;
using UnityEngine.UIElements;

namespace NeroWeNeed.Commons.Editor {

    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ValueTypeFieldDrawerAttribute : Attribute {
        public Type Type { get; }

        public ValueTypeFieldDrawerAttribute(Type type = null) {
            Type = type;


        }
    }


    public abstract class ValueTypeFieldDrawer {
        public abstract VisualElement CreateElement(Type type, object initial);
    }

}