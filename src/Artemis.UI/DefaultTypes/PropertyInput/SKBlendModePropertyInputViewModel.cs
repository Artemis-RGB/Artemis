using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class SKBlendModePropertyInputViewModel : PropertyInputViewModel<SKBlendMode>
{
    private FolderBlendMode _blendMode;

    public SKBlendModePropertyInputViewModel(LayerProperty<SKBlendMode> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        // The picker works with folder blend modes, which are the Skia ones plus pass through
        this.WhenAnyValue(vm => vm.InputValue).Subscribe(v => BlendMode = FolderGeneralProperties.FromSKBlendMode(v));
        this.WhenAnyValue(vm => vm.BlendMode).Subscribe(v =>
        {
            if (v != FolderBlendMode.PassThrough)
                InputValue = FolderGeneralProperties.ToSKBlendMode(v);
        });
    }

    public FolderBlendMode BlendMode
    {
        get => _blendMode;
        set => this.RaiseAndSetIfChanged(ref _blendMode, value);
    }
}
