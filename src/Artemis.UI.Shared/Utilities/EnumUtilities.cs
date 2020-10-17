using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Provides utilities for display enums in a human readable form
    /// </summary>
    public static class EnumUtilities
    {
        /// <summary>
        ///     Creates a list containing a <see cref="ValueDescription" /> for each value in the enum type
        /// </summary>
        /// <param name="t">The enum type to create value descriptions for</param>
        /// <returns>A list containing a <see cref="ValueDescription" /> for each value in the enum type</returns>
        public static IEnumerable<ValueDescription> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Select(e => new ValueDescription {Value = e, Description = e.Humanize()}).ToList();
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

    public class ValueDescription
    {
        public object Value { get; set; }
        public string Description { get; set; }
    }
}