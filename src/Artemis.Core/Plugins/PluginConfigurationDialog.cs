using System;

namespace Artemis.Core.Plugins
{
    /// <inheritdoc />
    public class PluginConfigurationDialog<T> : PluginConfigurationDialog where T : PluginConfigurationViewModel
    {
        /// <inheritdoc />
        public override Type Type => typeof(T);
    }

    /// <summary>
    ///     Describes a configuration dialog for a specific plugin
    /// </summary>
    public abstract class PluginConfigurationDialog
    {
        /// <summary>
        ///     The layer brush this dialog belongs to
        /// </summary>
        internal Plugin Plugin { get; set; }

        /// <summary>
        ///     The type of view model the tab contains
        /// </summary>
        public abstract Type Type { get; }
    }
}