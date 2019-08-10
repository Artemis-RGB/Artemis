using System;
using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    public class PluginSettingEntity
    {
        public Guid PluginGuid { get; set; }
        public string Name { get; set; }
        
        public string Value { get; set; }
    }
}