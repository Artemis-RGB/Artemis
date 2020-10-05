using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        ///     Gets a human readable description of the given enum value
        /// </summary>
        /// <param name="value">The value to get a description for</param>
        /// <returns>A human readable description of the given enum value</returns>
        public static string Description(this Enum value)
        {
            object[] attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
                return (attributes.First() as DescriptionAttribute).Description;

            // If no description is found, the least we can do is replace underscores with spaces
            // You can add your own custom default formatting logic here
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(ti.ToLower(value.ToString().Replace("_", " ")));
        }

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