﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Extensions;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree;

public class LayerTreeItemViewModel : TreeItemViewModel
{
    private readonly IDeviceService _deviceService;

    public LayerTreeItemViewModel(TreeItemViewModel? parent,
        Layer layer,
        IWindowService windowService,
        IProfileEditorService profileEditorService,
        IDeviceService deviceService,
        IProfileEditorVmFactory profileEditorVmFactory)
        : base(parent, layer, windowService, deviceService, profileEditorService, profileEditorVmFactory)
    {
        _deviceService = deviceService;
        Layer = layer;
    }

    public Layer Layer { get; }

    #region Overrides of TreeItemViewModel

    /// <inheritdoc />
    protected override async Task ExecuteDuplicate()
    {
        await ProfileEditorService.SaveProfileAsync();

        LayerEntity copy = CoreJson.DeserializeObject<LayerEntity>(CoreJson.SerializeObject(Layer.LayerEntity))!;
        copy.Id = Guid.NewGuid();
        copy.Name = Layer.Parent.GetNewFolderName(copy.Name + " - copy");

        Layer copied = new(Layer.Profile, Layer.Parent, copy, true);
        ProfileEditorService.ExecuteCommand(new AddProfileElement(copied, Layer.Parent, Layer.Order - 1));
        Layer.Profile.PopulateLeds(_deviceService.EnabledDevices);
    }

    protected override async Task ExecuteCopy()
    {
        await ProfileEditorService.SaveProfileAsync();
        await Layer.CopyToClipboard();
    }

    protected override async Task ExecutePaste()
    {
        if (Layer.Parent is not Folder parent)
            return;
        RenderProfileElement? pasted = await parent.PasteChildFromClipboard();
        if (pasted == null)
            return;

        // If the target contains an element with the same name, affix with " - copy"
        if (parent.Children.Any(c => c.Name == pasted.Name))
            pasted.Name = parent.GetNewLayerName(pasted.Name + " - copy");

        ProfileEditorService.ExecuteCommand(new AddProfileElement(pasted, parent, Layer.Order - 1));
        Layer.Profile.PopulateLeds(_deviceService.EnabledDevices);
    }

    /// <inheritdoc />
    public override bool SupportsChildren => false;

    #endregion
}