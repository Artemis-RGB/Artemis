using System;

namespace Artemis.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsGenericType(this Type type, Type genericType)
        {
            if (type == null)
                return false;

            return type.BaseType?.GetGenericTypeDefinition() == genericType;
        }
    }
}