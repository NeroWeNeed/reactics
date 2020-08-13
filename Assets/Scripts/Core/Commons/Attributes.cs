using System;
using Unity.Entities;
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

}