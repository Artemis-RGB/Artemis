using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class ColorGradientPropertyInputViewModel : PropertyInputViewModel<ColorGradient>
{
    private List<ColorGradientStop>? _originalStops;

    public ColorGradientPropertyInputViewModel(LayerProperty<ColorGradient> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
    }
    
    #region Overrides of PropertyInputViewModel<ColorGradient>

    /// <inheritdoc />
    public override void StartPreview()
    {
        _originalStops = InputValue?.Select(s => new ColorGradientStop(s.Color, s.Position)).ToList();
    }

    /// <inheritdoc />
    protected override void ApplyInputValue()
    {
        // Don't do anything, ColorGradient is a reference type and will update regardless
    }

    /// <inheritdoc />
    public override void ApplyPreview()
    {
        // If the new stops are equal to the old ones, nothing changes
        if (InputValue == null || _originalStops == null || !HasPreviewChanges())
            return;
        
        ProfileEditorService.ExecuteCommand(new UpdateColorGradient(InputValue, InputValue.ToList(), _originalStops));
        _originalStops = null;
    }

    /// <inheritdoc />
    public override void DiscardPreview()
    {
        if (InputValue == null || _originalStops == null)
            return;

        // Put the old gradient back
        InputValue.Clear();
        foreach (ColorGradientStop colorGradientStop in _originalStops)
            InputValue.Add(colorGradientStop);
        _originalStops = null;
    }

    private bool HasPreviewChanges()
    {
        if (InputValue == null || _originalStops == null)
            return false;
        
        if (InputValue.Count != _originalStops.Count)
            return true;

        for (int i = 0; i < InputValue.Count; i++)
            if (!Equals(InputValue[i], _originalStops[i]))
                return true;

        return false;
    }
    
    #endregion
}