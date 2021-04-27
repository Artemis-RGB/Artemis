using System;

namespace Artemis.Core
{
    /// <summary>
    ///     An exception thrown when a plugin prerequisite-related error occurs
    /// </summary>
    public class ArtemisPluginPrerequisiteException : Exception
    {
        internal ArtemisPluginPrerequisiteException(Plugin plugin, PluginPrerequisite? pluginPrerequisite)
        {
            Plugin = plugin;
            PluginPrerequisite = pluginPrerequisite;
        }

        internal ArtemisPluginPrerequisiteException(Plugin plugin, PluginPrerequisite? pluginPrerequisite, string message) : base(message)
        {
            Plugin = plugin;
            PluginPrerequisite = pluginPrerequisite;
        }

        internal ArtemisPluginPrerequisiteException(Plugin plugin, PluginPrerequisite? pluginPrerequisite, string message, Exception inner) : base(message, inner)
        {
            Plugin = plugin;
            PluginPrerequisite = pluginPrerequisite;
        }

        /// <summary>
        ///     Gets the plugin the error is related to
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets the plugin prerequisite the error is related to
        /// </summary>
        public PluginPrerequisite? PluginPrerequisite { get; }
    }
}