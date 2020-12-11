using System;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Core.Commons {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class MaxAttribute : Attribute {
        public float max;

        public MaxAttribute(float max) {
            this.max = max;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class UpdateInGroupFirstAttribute : UpdateInGroupAttribute {
        public UpdateInGroupFirstAttribute(Type groupType) : base(groupType) {
            OrderFirst = true;
        }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class UpdateInGroupLastAttribute : UpdateInGroupAttribute {
        public UpdateInGroupLastAttribute(Type groupType) : base(groupType) {
            OrderLast = true;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface)]
    public sealed class TypeColorAttribute : Attribute {

        public Color Color { get; }
        public TypeColorAttribute(string color) {
            Color = GeneralCommons.ParseColor(color);
        }
        public TypeColorAttribute(float r, float g, float b, float a = 1) {
            Color = new Color(r, g, b, a);
        }
        public TypeColorAttribute(int r, int g, int b, int a = 255) : this(r / 255f, g / 255f, b / 255f, a / 255f) { }
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class ConcreteTypeColorAttribute : Attribute {

        public Color Color { get; }

        public Type[] GenericDefinition { get; }
        public ConcreteTypeColorAttribute(string color, params Type[] genericDefinition) {
            Color = GeneralCommons.ParseColor(color);
            GenericDefinition = genericDefinition;
        }
        public ConcreteTypeColorAttribute(float r, float g, float b, float a, params Type[] genericDefinition) {
            Color = new Color(r, g, b, a);
            GenericDefinition = genericDefinition;
        }
        public ConcreteTypeColorAttribute(float r, float g, float b, params Type[] genericDefinition) : this(r, g, b, 1f, genericDefinition) { }
        public ConcreteTypeColorAttribute(int r, int g, int b, params Type[] genericDefinition) : this(r / 255f, g / 255f, b / 255f, 1f, genericDefinition) { }
        public ConcreteTypeColorAttribute(int r, int g, int b, int a, params Type[] genericDefinition) : this(r / 255f, g / 255f, b / 255f, a / 255f, genericDefinition) { }

    }

    public interface Identifiable {
        string Identifier { get; }
    }

}