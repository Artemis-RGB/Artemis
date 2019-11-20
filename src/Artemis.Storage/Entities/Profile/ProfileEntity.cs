using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile
{
    public class ProfileEntity
    {
        public ProfileEntity()
        {
            Folders = new List<FolderEntity>();
            Layers = new List<LayerEntity>();
        }

        public Guid Id { get; set; }
        public Guid PluginGuid { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; set; }

        public List<FolderEntity> Folders { get; set; }
        public List<LayerEntity> Layers { get; set; }
    }
}