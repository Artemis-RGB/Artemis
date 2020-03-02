using System;

namespace Artemis.Storage.Entities.Plugins
{
    /// <summary>
    ///     Represents the configuration of a plugin, each plugin has one configuration
    /// </summary>
    public class PluginEntity
    {
        public Guid Id { get; set; }
        public Guid PluginGuid { get; set; }

        public bool IsEnabled { get; set; }
        public bool LastEnableSuccessful { get; set; }
    }
}