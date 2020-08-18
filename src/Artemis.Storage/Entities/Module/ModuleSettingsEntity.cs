using System;

namespace Artemis.Storage.Entities.Module
{
    public class ModuleSettingsEntity
    {
        public Guid PluginGuid { get; set; }
        public int PriorityCategory { get; set; }
        public int Priority { get; set; }
    }
}