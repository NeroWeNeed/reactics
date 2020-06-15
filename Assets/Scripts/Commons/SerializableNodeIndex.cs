using System;

namespace Reactics.Commons
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeNodeIndex : Attribute
    {
        public Type nodeType;

        public SerializeNodeIndex(Type nodeType)
        {
            this.nodeType = nodeType;
        }
    }
}