using System.Reflection;
using System.Collections.Generic;
using System;

namespace Reactics.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class EffectParameter : Attribute
    {

    }

    public static class EffectFactory
    {

        public static T Create<T>(Dictionary<string, object> parameters) where T : unmanaged => (T)Create(typeof(T), parameters);
        public static object Create(Type type, Dictionary<string, object> parameters)
        {

            var effect = Activator.CreateInstance(type);
            var fields = type.GetFields();
            var effectTypedRef = TypedReference.MakeTypedReference(effect, fields);

            foreach (var field in fields)
            {

                if (field.GetCustomAttribute<EffectParameter>() != null && parameters.TryGetValue(field.Name, out object fieldValue))
                {

                    if (!field.FieldType.IsEquivalentTo(fieldValue.GetType()))
                        throw new ArgumentException($"Mismatching type between field {field.Name} with type {field.FieldType} and value type {fieldValue.GetType()}");
                    field.SetValueDirect(effectTypedRef, fieldValue);
                }
            }
            return effect;

        }

    }
}