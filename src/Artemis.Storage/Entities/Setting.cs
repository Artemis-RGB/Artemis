using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    internal class Setting
    {
        [Key]
        public string Name { get; set; }

        public string Value { get; set; }
    }
}