using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an attribute that describes a plugin feature
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginFeatureAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the user-friendly name for this property, shown in the UI.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        ///     Gets or sets the user-friendly description for this property, shown in the UI.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     The plugins display icon that's shown in the settings see <see href="https://materialdesignicons.com" /> for
        ///     available icons
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        ///     Marks the feature to always be enabled as long as the plugin is enabled
        /// </summary>
        public bool AlwaysEnabled { get; set; }
    }
}