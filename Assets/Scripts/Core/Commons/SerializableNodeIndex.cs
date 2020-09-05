using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reactics.Core.Commons {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SerializeNodeIndex : Attribute {
        public Type nodeType;

        public SerializeNodeIndex(Type nodeType) {
            this.nodeType = nodeType;
        }
    }
    [Serializable]
    public struct IndexReference {
        public int index;

        public IndexReference(int index) {
            this.index = index;
        }
    }


}