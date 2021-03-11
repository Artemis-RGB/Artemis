using System;

namespace Artemis.Storage.Entities.Plugins
{
    /// <summary>
    ///     Represents the setting of a plugin, a plugin can have multiple settings
    /// </summary>
    public class PluginSettingEntity
    {
        public Guid Id { get; set; }
        public Guid PluginGuid { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    ///     Represents a queued action for a plugin
    /// </summary>
    public abstract class PluginQueuedActionEntity
    {
        public Guid Id { get; set; }
        public Guid PluginGuid { get; set; }
    }

    /// <summary>
    ///     Represents a queued delete action for a plugin
    /// </summary>
    public class PluginQueuedDeleteEntity : PluginQueuedActionEntity
    {
        public string Directory { get; set; }
    }
}