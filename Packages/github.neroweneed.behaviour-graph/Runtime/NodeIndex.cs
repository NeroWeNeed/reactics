using System;
using NeroWeNeed.Commons;

namespace NeroWeNeed.BehaviourGraph {
    /// <summary>
    /// Special Handle Struct for referencing nodes in a Behaviour Graph.
    /// </summary>

    [HiddenInValueTypeElement]
    public struct NodeIndex {
        public sbyte value;
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class EmbedInBehaviourGraphAttribute : System.Attribute { }

}