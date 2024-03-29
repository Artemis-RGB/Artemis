using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.UI.Exceptions;

namespace Artemis.UI.Models;

public class FolderClipboardModel: IClipboardModel
{
    public FolderClipboardModel(Folder folder)
    {
        FolderEntity = folder.FolderEntity;
        Folders = new List<FolderEntity>();
        Layers = new List<LayerEntity>();
        foreach (Folder allFolder in folder.GetAllFolders())
            Folders.Add(allFolder.FolderEntity);
        foreach (Layer allLayer in folder.GetAllLayers())
            Layers.Add(allLayer.LayerEntity);
    }

    [JsonConstructor]
    public FolderClipboardModel()
    {
        FolderEntity = null;
        Folders = new List<FolderEntity>();
        Layers = new List<LayerEntity>();
    }

    public FolderEntity? FolderEntity { get; set; }
    public List<FolderEntity> Folders { get; set; }
    public List<LayerEntity> Layers { get; set; }
    public bool HasBeenPasted { get; set; }

    public Folder Paste(Profile profile, ProfileElement parent)
    {
        if (FolderEntity == null)
            throw new ArtemisUIException("Couldn't paste folder because FolderEntity deserialized as null");
        if (HasBeenPasted)
            throw new ArtemisUIException("Clipboard model can only be pasted once");

        HasBeenPasted = true;

        // Generate new GUIDs
        ReplaceGuid(FolderEntity);
        foreach (FolderEntity folderEntity in Folders)
            ReplaceGuid(folderEntity);
        foreach (LayerEntity layerEntity in Layers)
            ReplaceGuid(layerEntity);

        // Inject the pasted elements into the profile
        profile.ProfileEntity.Folders.AddRange(Folders);
        profile.ProfileEntity.Layers.AddRange(Layers);

        // Let the folder initialize and load as usual
        FolderEntity.Name += " - copy";
        Folder folder = new(profile, parent, FolderEntity);
        return folder;
    }

    private void ReplaceGuid(RenderElementEntity parent)
    {
        Guid old = parent.Id;
        parent.Id = Guid.NewGuid();

        foreach (FolderEntity child in Folders)
        {
            if (child.ParentId == old)
                child.ParentId = parent.Id;
        }

        foreach (LayerEntity child in Layers)
        {
            if (child.ParentId == old)
                child.ParentId = parent.Id;
        }
    }
}