using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeroWeNeed.Commons.Editor {
    public sealed class TypeFilter {
        public TypeFilterAttribute[] filterAttributes;

        public TypeFilter(TypeFilterAttribute[] filterAttributes) {
            this.filterAttributes = filterAttributes;
            Array.Sort(filterAttributes);
        }

        public override bool Equals(object obj) {
            return obj is TypeFilter filter && Array.Equals(filterAttributes, filter.filterAttributes);
        }

        public override int GetHashCode() {
            return -1995125846 + EqualityComparer<TypeFilterAttribute[]>.Default.GetHashCode(filterAttributes);
        }
        public bool IsValid(Type type) {
            foreach (var filterAttribute in filterAttributes) {
                if (!filterAttribute.IsValid(type))
                    return false;
            }
            return true;
        }
        public List<Type> CollectTypes() {
            var output = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.GetCustomAttribute<SearchableAssemblyAttribute>() != null) {
                    foreach (var type in assembly.GetTypes()) {
                        if (IsValid(type))
                            output.Add(type);
                    }
                }
            }
            return output;
        }
        public List<SerializableType> CollectTypesAsSerializable() {
            var output = new List<SerializableType>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                if (assembly.GetCustomAttribute<SearchableAssemblyAttribute>() != null) {
                    foreach (var type in assembly.GetTypes()) {
                        if (IsValid(type))
                            output.Add(new SerializableType(type));
                    }
                }
            }
            return output;
        }
    }
}