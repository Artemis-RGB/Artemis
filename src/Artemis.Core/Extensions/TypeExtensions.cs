using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Artemis.Core
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, List<Type>> PrimitiveTypeConversions = new Dictionary<Type, List<Type>>
        {
            {typeof(decimal), new List<Type> {typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char)}},
            {typeof(double), new List<Type> {typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float)}},
            {typeof(float), new List<Type> {typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(char), typeof(float)}},
            {typeof(ulong), new List<Type> {typeof(byte), typeof(ushort), typeof(uint), typeof(char)}},
            {typeof(long), new List<Type> {typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(char)}},
            {typeof(uint), new List<Type> {typeof(byte), typeof(ushort), typeof(char)}},
            {typeof(int), new List<Type> {typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(char)}},
            {typeof(ushort), new List<Type> {typeof(byte), typeof(char)}},
            {typeof(short), new List<Type> {typeof(byte)}}
        };

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

        public static bool TypeIsNumber(this Type type)
        {
            return type == typeof(sbyte)
                   || type == typeof(byte)
                   || type == typeof(short)
                   || type == typeof(ushort)
                   || type == typeof(int)
                   || type == typeof(uint)
                   || type == typeof(long)
                   || type == typeof(ulong)
                   || type == typeof(float)
                   || type == typeof(double)
                   || type == typeof(decimal);
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

        // From https://stackoverflow.com/a/2224421/5015269 but inverted and renamed to match similar framework methods
        /// <summary>
        ///     Determines whether an instance of a specified type can be casted to a variable of the current type
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static bool IsCastableFrom(this Type to, Type from)
        {
            if (to.TypeIsNumber() && from.TypeIsNumber())
                return true;
            if (to.IsAssignableFrom(from))
                return true;
            if (PrimitiveTypeConversions.ContainsKey(to) && PrimitiveTypeConversions[to].Contains(from))
                return true;
            bool castable = from.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Any(
                    m => m.ReturnType == to &&
                         (m.Name == "op_Implicit" ||
                          m.Name == "op_Explicit")
                );
            return castable;
        }

        /// <summary>
        ///     Returns the default value of the given type
        /// </summary>
        public static object GetDefault(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        ///     Determines whether the given type is a generic enumerable
        /// </summary>
        public static bool IsGenericEnumerable(this Type type)
        {
            return type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        /// <summary>
        ///     Determines the type of the provided generic enumerable type
        /// </summary>
        public static Type? GetGenericEnumerableType(this Type type)
        {
            Type enumerableType = type.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableType?.GenericTypeArguments[0];
        }
    }
}