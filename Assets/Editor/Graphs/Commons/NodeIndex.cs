using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.UnityConverters;
using Reactics.Core.Commons;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Reactics.Editor.Graph {

    /*     [Serializable]
        public struct NodeIndex : IEquatable<NodeIndex> {
            public Type type;

            public Guid node;
            public NodeIndex(Type portType) {
                this.type = portType;
                node = Guid.Empty;
            }

            public NodeIndex(Type portType, Guid node) : this(portType) {
                this.node = node;
            }

            public override bool Equals(object obj) {
                return obj is NodeIndex index &&
                       EqualityComparer<Type>.Default.Equals(type, index.type) &&
                       EqualityComparer<Guid>.Default.Equals(node, index.node);
            }

            public bool Equals(NodeIndex other) {
                return EqualityComparer<Type>.Default.Equals(type, other.type) &&
                       EqualityComparer<Guid>.Default.Equals(node, other.node);
            }

            public override int GetHashCode() {
                int hashCode = -1114374285;
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
                hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(node);
                return hashCode;
            }

            public override string ToString() {
                return "NodeIndex(" + type.FullName + ", " + node + ")";
            }
        } */


}