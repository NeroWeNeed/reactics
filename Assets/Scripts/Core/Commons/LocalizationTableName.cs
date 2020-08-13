using System;

namespace Reactics.Core.Commons {

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class LocalizationTableNameAttribute : Attribute {
        public string name;
        public LocalizationTableNameAttribute(string name) {
            this.name = name;
        }
    }
}