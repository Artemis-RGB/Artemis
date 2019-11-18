using System;
using System.ComponentModel.DataAnnotations;

namespace Artemis.Storage.Entities
{
    public class ProfileEntity
    {
        [Key]
        public string Guid { get; set; }

        public Guid PluginGuid { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; set; }

        public int RootFolderId { get; set; }
        public virtual FolderEntity RootFolder { get; set; }
    }
}