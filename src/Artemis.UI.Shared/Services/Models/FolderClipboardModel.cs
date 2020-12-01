using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.UI.Shared.Services.Models
{
    internal class FolderClipboardModel
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

        // ReSharper disable once UnusedMember.Global - For JSON.NET
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
                throw new ArtemisSharedUIException("Couldn't paste folder because FolderEntity deserialized as null");
            if (HasBeenPasted)
                throw new ArtemisSharedUIException("Clipboard model can only be pasted once");

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
            Folder folder = new Folder(profile, parent, FolderEntity);
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
}