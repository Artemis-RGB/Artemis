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

        public static bool IsStruct(this Type source)
        {
            return source.IsValueType && !source.IsPrimitive && !source.IsEnum;
        }

        public static bool IsNumber(this object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }
    }
}