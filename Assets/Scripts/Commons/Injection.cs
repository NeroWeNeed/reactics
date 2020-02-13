using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Reactics.Util
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Constructor)]
    public class Inject : Attribute
    {
        public readonly string name;

        public Inject(string name = "")
        {
            this.name = name;
        }
    }





    public class InjectActivator
    {
        private Dictionary<string, Dictionary<InjectionProperty, dynamic>> properties = new Dictionary<string, Dictionary<InjectionProperty, dynamic>>();
        public T CreateInstance<T>()
        {
            return (T)CreateInstance(typeof(T));
        }
        public object CreateInstance(Type type)
        {

            ConstructorInfo constructor = type.GetConstructors().Where(x => x.GetCustomAttribute<Inject>() != null).FirstOrDefault();
            if (constructor == null)
                return Activator.CreateInstance(type);
            Inject constructorAttr = constructor.GetCustomAttribute<Inject>();
            string contextName = constructorAttr.name;
            Dictionary<InjectionProperty, dynamic> contextProperties;
            if (properties.ContainsKey(contextName))
            {
                contextProperties = new Dictionary<InjectionProperty, dynamic>();
                properties[contextName] = contextProperties;
            }
            else
            {
                contextProperties = properties[contextName];
            }
            properties.TryGetValue(contextName, out contextProperties);
            InjectionProperty property;
            Inject parameterAttr;
            object[] arguments = constructor.GetParameters().Select(parameter =>
            {
                if (parameter.DefaultValue != DBNull.Value)
                    return parameter.DefaultValue;
                parameterAttr = parameter.GetCustomAttribute<Inject>();
                property = new InjectionProperty(parameter.ParameterType, parameterAttr == null ? "" : parameterAttr.name);
                if (!contextProperties.ContainsKey(property))
                {
                    object t = Activator.CreateInstance(property.type);
                    contextProperties[property] = t;
                    return t;
                }
                else
                {
                    return contextProperties[property];
                }

            }).ToArray();
            return constructor.Invoke(arguments);
        }
        public void Inject(string contextName, string injectionName, object value)
        {
            if (!properties.ContainsKey(contextName))
                properties[contextName] = new Dictionary<InjectionProperty, dynamic>();
            properties[contextName][new InjectionProperty(value.GetType(), injectionName)] = value;
        }
        public void Inject(string injectionName, object value)
        {
            Inject("", injectionName, value);
        }
        public void Inject(object value)
        {
            Inject("", "", value);
        }
        private struct InjectionProperty
        {
            public readonly Type type;
            public readonly string name;

            public InjectionProperty(Type type, string name = "")
            {
                this.type = type;
                this.name = name;
            }

            public override bool Equals(object obj)
            {
                return obj is InjectionProperty property &&
                       EqualityComparer<Type>.Default.Equals(type, property.type) &&
                       name == property.name;
            }

            public override int GetHashCode()
            {
                int hashCode = -1890651077;
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(type);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
                return hashCode;
            }
        }
    }


}