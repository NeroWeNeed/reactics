using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace NeroWeNeed.Commons {
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class SearchableAssemblyAttribute : Attribute {
        public Type Type { get; }

        public SearchableAssemblyAttribute(Type type = null) {
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public abstract class TypeFilterAttribute : Attribute, IComparable<TypeFilterAttribute> {
        public abstract Type ComparisonType { get; }
        public int CompareTo(TypeFilterAttribute other) {
            return Comparer<string>.Default.Compare(ComparisonType?.AssemblyQualifiedName, other.ComparisonType?.AssemblyQualifiedName);
        }

        public abstract bool IsValid(Type type);
    }
    /// <summary>
    /// Only Show types that have the following type as a supertype.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class SuperTypeFilterAttribute : TypeFilterAttribute {
        private readonly Type type;
        public bool AllowSuperType { get; set; }
        public override Type ComparisonType { get => type; }

        public SuperTypeFilterAttribute(Type type) {
            this.type = type;
        }

        public override bool Equals(object obj) {
            return obj is SuperTypeFilterAttribute attribute &&
                   EqualityComparer<Type>.Default.Equals(type, attribute.type);
        }

        public override int GetHashCode() {
            int hashCode = 1064687083;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
            return hashCode;
        }

        public override bool IsValid(Type type) {
            return this.type.IsAssignableFrom(type) && (AllowSuperType || !type.Equals(this.type));

        }
    }
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AttributeTypeFilterAttribute : TypeFilterAttribute {

        public override Type ComparisonType { get; }

        public AttributeTypeFilterAttribute(Type type) {
            this.ComparisonType = type;
        }

        public override bool IsValid(Type type) {
            return type.GetCustomAttribute(this.ComparisonType) != null;
        }
    }

    public sealed class ParameterlessConstructorFilterAttribute : TypeFilterAttribute {

        public override Type ComparisonType { get => typeof(ParameterlessConstructorFilterAttribute); }

        public override bool IsValid(Type type) {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }
    }
    public sealed class UnmanagedFilterAttribute : TypeFilterAttribute {
        public override Type ComparisonType { get => typeof(UnmanagedFilterAttribute); }
        public override bool IsValid(Type type) {
            return UnsafeUtility.IsUnmanaged(type);
        }
    }
    public sealed class BlittableFilterAttribute : TypeFilterAttribute {
        public override Type ComparisonType { get => typeof(BlittableFilterAttribute); }
        public override bool IsValid(Type type) {
            return UnsafeUtility.IsBlittable(type);
        }
    }
    public sealed class ConcreteTypeFilterAttribute : TypeFilterAttribute {
        public override Type ComparisonType { get => typeof(ConcreteTypeFilterAttribute); }

        public override bool IsValid(Type type) {
            return !type.IsGenericTypeDefinition;
        }
    }

}
