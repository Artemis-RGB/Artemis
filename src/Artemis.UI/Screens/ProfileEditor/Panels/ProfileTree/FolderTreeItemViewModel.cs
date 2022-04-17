using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using Artemis.UI.Extensions;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree;

public class FolderTreeItemViewModel : TreeItemViewModel
{
    public FolderTreeItemViewModel(TreeItemViewModel? parent,
        Folder folder,
        IWindowService windowService,
        IProfileEditorService profileEditorService,
        ILayerBrushService layerBrushService,
        IProfileEditorVmFactory profileEditorVmFactory,
        IRgbService rgbService)
        : base(parent, folder, windowService, profileEditorService, rgbService, layerBrushService, profileEditorVmFactory)
    {
        Folder = folder;
    }

    public Folder Folder { get; }


    #region Overrides of TreeItemViewModel

    protected override async Task ExecuteDuplicate()
    {
        await ProfileEditorService.SaveProfileAsync();

        FolderEntity copy = CoreJson.DeserializeObject<FolderEntity>(CoreJson.SerializeObject(Folder.FolderEntity, true), true)!;
        copy.Id = Guid.NewGuid();
        copy.Name = Folder.Parent.GetNewFolderName(copy.Name + " - copy");

        Folder copied = new(Folder.Profile, Folder.Parent, copy);
        ProfileEditorService.ExecuteCommand(new AddProfileElement(copied, Folder.Parent, Folder.Order - 1));
    }

    protected override async Task ExecuteCopy()
    {
        await ProfileEditorService.SaveProfileAsync();
        await Folder.CopyToClipboard();
    }

    protected override async Task ExecutePaste()
    {
        if (Folder.Parent is not Folder parent)
            return;
        RenderProfileElement? pasted = await parent.PasteChildFromClipboard();
        if (pasted == null)
            return;

        // If the target contains an element with the same name, affix with " - copy"
        if (parent.Children.Any(c => c.Name == pasted.Name))
            pasted.Name = parent.GetNewFolderName(pasted.Name + " - copy");

        ProfileEditorService.ExecuteCommand(new AddProfileElement(pasted, parent, Folder.Order - 1));
    }

    /// <inheritdoc />
    public override bool SupportsChildren => true;

    #endregion
}