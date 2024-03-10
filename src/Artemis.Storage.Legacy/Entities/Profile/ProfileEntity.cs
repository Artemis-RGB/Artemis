﻿using Artemis.Storage.Legacy.Entities.General;

namespace Artemis.Storage.Legacy.Entities.Profile;

internal class ProfileEntity
{
    public ProfileEntity()
    {
        Folders = new List<FolderEntity>();
        Layers = new List<LayerEntity>();
        ScriptConfigurations = new List<ScriptConfigurationEntity>();
    }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public bool IsFreshImport { get; set; }

    public List<FolderEntity> Folders { get; set; }
    public List<LayerEntity> Layers { get; set; }
    public List<ScriptConfigurationEntity> ScriptConfigurations { get; set; }

    public void UpdateGuid(Guid guid)
    {
        Guid oldGuid = Id;
        Id = guid;

        FolderEntity? rootFolder = Folders.FirstOrDefault(f => f.ParentId == oldGuid);
        if (rootFolder != null)
            rootFolder.ParentId = Id;
    }
}