using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;

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

        private static readonly Dictionary<Type, string> TypeKeywords = new Dictionary<Type, string>
        {
            {typeof(bool), "bool"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(char), "char"},
            {typeof(decimal), "decimal"},
            {typeof(double), "double"},
            {typeof(float), "float"},
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(object), "object"},
            {typeof(string), "string"}
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
                .Any(m => m.ReturnType == to && (m.Name == "op_Implicit" || m.Name == "op_Explicit"));
            return castable;
        }

        /// <summary>
        ///     Scores how well the two types can be casted from one to another, 5 being a perfect match and 0 being not castable
        ///     at all
        /// </summary>
        /// <returns></returns>
        public static int ScoreCastability(this Type to, Type from)
        {
            if (to == from)
                return 5;
            if (to.TypeIsNumber() && from.TypeIsNumber())
                return 4;
            if (PrimitiveTypeConversions.ContainsKey(to) && PrimitiveTypeConversions[to].Contains(from))
                return 3;
            bool castable = from.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Any(m => m.ReturnType == to && (m.Name == "op_Implicit" || m.Name == "op_Explicit"));
            if (castable)
                return 2;
            if (to.IsAssignableFrom(from))
                return 1;
            return 0;
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
            // String is an IEnumerable to be fair, but not for us
            if (type == typeof(string))
                return false;
            // It may actually be one instead of implementing one ;)
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return true;

            return type.GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        /// <summary>
        ///     Determines the type of the provided generic enumerable type
        /// </summary>
        public static Type? GetGenericEnumerableType(this Type type)
        {
            // It may actually be one instead of implementing one ;)
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return type.GenericTypeArguments[0];

            Type enumerableType = type.GetInterfaces().FirstOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableType?.GenericTypeArguments[0];
        }

        /// <summary>
        ///     Determines a display name for the given type
        /// </summary>
        /// <param name="type">The type to determine the name for</param>
        /// <param name="humanize">Whether or not to humanize the result, defaults to false</param>
        /// <returns></returns>
        public static string GetDisplayName(this Type type, bool humanize = false)
        {
            if (!type.IsGenericType)
            {
                string displayValue = TypeKeywords.TryGetValue(type, out string? keyword) ? keyword! : type.Name;
                return humanize ? displayValue.Humanize() : displayValue;
            }

            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(Nullable<>))
                return type.GenericTypeArguments[0].GetDisplayName(humanize) + "?";

            string stripped = genericTypeDefinition.Name.Split('`')[0];
            return $"{stripped}<{string.Join(", ", type.GenericTypeArguments.Select(t => t.GetDisplayName(humanize)))}>";
        }
    }
}