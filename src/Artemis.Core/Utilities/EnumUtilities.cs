using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides utilities for display enums in a human readable form
    /// </summary>
    public static class EnumUtilities
    {
        /// <summary>
        ///     Creates a list containing a tuple for each value in the enum type
        /// </summary>
        /// <typeparam name="T">The enum type to create value descriptions for</typeparam>
        /// <returns>A list containing a value-description tuple for each value in the enum type</returns>
        public static List<(T, string)> GetAllValuesAndDescriptions<T>() where T : struct, Enum
        {
            return Enum.GetValues<T>().Select(e => (e, e.Humanize())).ToList();
        }

        /// <summary>
        ///     Creates a list containing a tuple for each value in the enum type
        /// </summary>
        /// <param name="t">The enum type to create value descriptions for</param>
        /// <returns>A list containing a value-description tuple  for each value in the enum type</returns>
        public static List<(Enum, string)> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{t} must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Select(e => (e, e.Humanize())).ToList();
        }

        /// <summary>
        ///     Humanizes the given enum value using the Humanizer library
        /// </summary>
        /// <param name="value">The enum value to humanize</param>
        /// <returns>A humanized string describing the given value</returns>
        public static string HumanizeValue(Enum value)
        {
            return value.Humanize();
        }
    }
}