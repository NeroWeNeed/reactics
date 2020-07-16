using System;
namespace Reactics.Commons {
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class MaxAttribute : Attribute {
        public float max;

        public MaxAttribute(float max) {
            this.max = max;
        }
    }

}