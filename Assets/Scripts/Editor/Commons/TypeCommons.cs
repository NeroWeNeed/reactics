using System.Collections.Generic;
using System;

namespace Reactics.Editor
{

    public static class TypeCommons
    {
        private static Dictionary<Type, Boolean> unmanagedTypes = new Dictionary<Type, bool>();
        public static bool IsUnmanaged(this Type type)
        {
            if (type.IsPrimitive || type.IsEnum || type.IsPointer)
            {
                return true;
            }
            else if (unmanagedTypes.TryGetValue(type, out bool value))
            {
                return value;
            }
            else if (type.IsValueType)
            {
                foreach (var field in type.GetFields())
                {
                    if (field.FieldType.Equals(type))
                        continue;
                    if (!IsUnmanaged(field.FieldType))
                    {
                        unmanagedTypes.Add(type, false);
                        return false;
                    }
                }
                unmanagedTypes.Add(type, true);
                return true;
            }
            else
            {

                return false;
            }

        }
    }
}