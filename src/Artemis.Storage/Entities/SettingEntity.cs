using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    internal class SettingEntity
    {
        [Key]
        public string Name { get; set; }

        public string Value { get; set; }
    }
}