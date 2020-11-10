using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Plugins
{
    /// <summary>
    ///     Represents the configuration of a plugin, each plugin has one configuration
    /// </summary>
    public class PluginEntity
    {
        public Guid Id { get; set; }
        public bool IsEnabled { get; set; }

        public List<PluginImplementationEntity> Implementations { get; set; }
    }

    /// <summary>
    ///     Represents the configuration of a plugin implementation, each implementation has one configuration
    /// </summary>
    public class PluginImplementationEntity
    {
        public string Type { get; set; }
        public bool IsEnabled { get; set; }
    }
}