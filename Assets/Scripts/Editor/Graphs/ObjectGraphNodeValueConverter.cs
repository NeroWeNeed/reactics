using System;
using System.Collections.Generic;
using System.Linq;
using Reactics.Commons;
using UnityEngine;

namespace Reactics.Editor.Graph {
    public abstract class ObjectGraphNodeValueConverter {
        public abstract object ToAliasUnTyped(object original, object data);
        public abstract object ToOriginalUnTyped(object alias, object data);
    }

    public abstract class ObjectGraphNodeValueConverter<TOriginal, TAlias> : ObjectGraphNodeValueConverter {
        public abstract TAlias ToAlias(TOriginal original, object data);
        public abstract TOriginal ToOriginal(TAlias alias, object data);
        public override object ToAliasUnTyped(object original, object data) => ToAlias((TOriginal)original, data);
        public override object ToOriginalUnTyped(object alias, object data) => ToOriginal((TAlias)alias, data);
    }
    public static class ObjectGraphNodeValueConverters {

        private static Converters converters = null;

        public static bool TryToConvertToAlias(object original, object data, out object result) {
            if (converters == null) {
                converters = new Converters();
            }
            if (converters.TryGetFromOriginal(original.GetType(), out ObjectGraphNodeValueConverter converter)) {
                result = converter.ToAliasUnTyped(original, data);
                return true;
            }
            else {
                result = original;
                return false;
            }
        }
        public static object ConvertToAlias(object original, object data) {
            if (converters == null) {
                converters = new Converters();
            }
            if (converters.TryGetFromOriginal(original.GetType(), out ObjectGraphNodeValueConverter converter)) {
                return converter.ToAliasUnTyped(original, data);
            }
            else {
                throw new ArgumentException("No ObjectGraphNodeValueConverter Converter Found.");
            }
        }
        public static object ConvertToOriginal(object alias, object data) {
            if (converters == null) {
                converters = new Converters();
            }
            if (converters.TryGetFromAlias(alias.GetType(), out ObjectGraphNodeValueConverter converter)) {
                return converter.ToOriginalUnTyped(alias, data);
            }
            else {
                throw new ArgumentException("No ObjectGraphNodeValueConverter Converter Found.");
            }
        }
        public static object TryToConvertToOriginal(object alias, object data, out object result) {
            if (converters == null) {
                converters = new Converters();
            }
            if (converters.TryGetFromAlias(alias.GetType(), out ObjectGraphNodeValueConverter converter)) {
                result = converter.ToOriginalUnTyped(alias, data);
                return true;
            }
            else {
                result = alias;
                return false;
            }
        }
        private class Converters {


            private readonly Dictionary<Type, ObjectGraphNodeValueConverter> aliasTypes = new Dictionary<Type, ObjectGraphNodeValueConverter>();
            private readonly Dictionary<Type, ObjectGraphNodeValueConverter> originalTypes = new Dictionary<Type, ObjectGraphNodeValueConverter>();

            public Converters() {

                foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                                            .Select(dll => dll.GetTypes()
                                                .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition && typeof(ObjectGraphNodeValueConverter).IsAssignableFrom(type))
                                            )
                                            .SelectMany(types => types)) {
                    var baseType = type.BaseType;
                    while (baseType != null) {
                        if (baseType.IsGenericType && typeof(ObjectGraphNodeValueConverter<,>).IsAssignableFrom(baseType.GetGenericTypeDefinition())) {
                            var converter = (ObjectGraphNodeValueConverter)Activator.CreateInstance(type);
                            originalTypes[baseType.GenericTypeArguments[0]] = converter;
                            aliasTypes[baseType.GenericTypeArguments[1]] = converter;
                            break;
                        }

                    }
                }
            }


            public bool TryGetFromAlias(Type type, out ObjectGraphNodeValueConverter result) {
                if (aliasTypes.TryGetValue(type, out result)) {
                    return true;
                }
                else {
                    foreach (var key in aliasTypes.Keys) {
                        if (key.IsAssignableFrom(type)) {
                            result = aliasTypes[key];
                            return true;
                        }
                    }
                    result = default;
                    return false;
                }
            }

            public bool TryGetFromOriginal(Type type, out ObjectGraphNodeValueConverter result) {
                if (originalTypes.TryGetValue(type, out result)) {
                    return true;
                }
                else {
                    foreach (var key in originalTypes.Keys) {
                        if (key.IsAssignableFrom(type)) {
                            result = originalTypes[key];
                            return true;
                        }
                    }
                    result = default;
                    return false;
                }
            }
        }
    }

    public class IndexReferenceConverter : ObjectGraphNodeValueConverter<IndexReference, NodeReference> {
        public override NodeReference ToAlias(IndexReference original, object data) {
            if (data is ObjectGraphSerializerPayload payload) {
                if (original.index >= 0 && original.index < payload.entries.Count)
                    return new NodeReference(payload.entries[original.index].key);
                else if (original.index == -2)
                    return new NodeReference(payload.graphView.MasterNode.viewDataKey);
            }
            return new NodeReference(null);
        }

        public override IndexReference ToOriginal(NodeReference alias, object data) {
            if (data is ObjectGraphSerializerPayload payload) {
                if (alias.nodeId == payload.graphView.MasterNode.viewDataKey)
                    return new IndexReference(-2);
                else
                    return new IndexReference(payload.entries.FindIndex((entry) => entry.key == alias.nodeId));
            }
            else {
                return new IndexReference(-1);
            }
        }
    }
}