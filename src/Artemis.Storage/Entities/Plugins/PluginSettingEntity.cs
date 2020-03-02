using System;

namespace Artemis.Storage.Entities.Plugins
{
    /// <summary>
    /// Represents the setting of a plugin, a plugin can have multiple settings
    /// </summary>
    public class PluginSettingEntity
    {
        public Guid Id { get; set; }
        public Guid PluginGuid { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}