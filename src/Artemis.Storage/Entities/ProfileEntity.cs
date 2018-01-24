using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    internal class ProfileEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string Module { get; set; }

        public int RootFolderId { get; set; }
        public virtual FolderEntity RootFolder { get; set; }
    }
}