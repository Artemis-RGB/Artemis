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

            return Enum.GetValues(t).Cast<Enum>().Select(e => new ValueDescription(e, e.Humanize())).ToList();
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

    /// <summary>
    ///     Represents a value and a description for an enum value
    /// </summary>
    public class ValueDescription
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ValueDescription"/> class
        /// </summary>
        /// <param name="value">The enum value</param>
        /// <param name="description">The description of the value</param>
        public ValueDescription(object value, string description)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }

        /// <summary>
        ///     The enum value
        /// </summary>
        public object Value { get; }

        /// <summary>
        ///     The description of the value
        /// </summary>
        public string Description { get; }
    }
}