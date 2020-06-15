using System.Collections.Generic;
using System;
using Unity.Entities;
using UnityEngine;

namespace Reactics.Commons
{
    /// <summary>
    /// Used to identify various methods or functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class Identifiable : Attribute
    {
        public readonly string value;

        public Identifiable(string value)
        {
            if (value.Length <= 0 || value.Length > 64)
            {
                throw new ArgumentException("Identifiers must be non-empty and less than 64 characters");
            }
            else
            {
                this.value = value;
            }
        }
    }

    
    [Serializable]
    public struct Identifier : IEquatable<Identifier>, IComparable<Identifier>
    {
        public static readonly Identifier Null = new Identifier(Guid.Empty);


        [SerializeField]
        private Guid value;

        public Identifier(Guid identifierNameSpace, string value)
        {
            this.value = DeterministicGuid.Create(identifierNameSpace, value);
        }
        private Identifier(Guid value)
        {
            this.value = value;
        }
        public int CompareTo(Identifier other)
        {
            return value.CompareTo(other.value);
        }

        public bool Equals(Identifier other)
        {
            return value.Equals(other.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
    public interface IdentifierSpace
    {
        Guid Namespace { get; }

    }

    public static class IdentifierExtensions
    {
        public static Identifier Create(this IdentifierSpace space, string value)
        {
            return new Identifier(space.Namespace, value);
        }
    }
#if UNITY_EDITOR
    public static class IdentifierNameCache
    {
        public static Dictionary<Identifier, string> Names = new Dictionary<Identifier, string>();
    }

#endif

}