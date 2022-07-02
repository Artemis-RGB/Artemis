using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Services.PropertyInput;
using ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class ColorGradientPropertyInputViewModel : PropertyInputViewModel<ColorGradient>
{
    private ColorGradient _colorGradient = null!;
    private ColorGradient? _originalGradient;

    public ColorGradientPropertyInputViewModel(LayerProperty<ColorGradient> layerProperty, IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
    }

    public ColorGradient ColorGradient
    {
        get => _colorGradient;
        set => this.RaiseAndSetIfChanged(ref _colorGradient, value);
    }

    protected override void OnInputValueChanged()
    {
        ColorGradient = new ColorGradient(InputValue);
    }

    #region Overrides of PropertyInputViewModel<ColorGradient>

    /// <inheritdoc />
    public override void StartPreview()
    {
        _originalGradient = LayerProperty.CurrentValue;

        // Set the property value to the gradient being edited by the picker, this will cause any updates to show right away because
        // ColorGradient is a reference type
        LayerProperty.CurrentValue = ColorGradient;

        // This won't fly if we ever support keyframes but at that point ColorGradient would have to be a value type anyway and this
        // whole VM no longer makes sense
    }

    /// <inheritdoc />
    protected override void ApplyInputValue()
    {
        // Don't do anything, ColorGradient is a reference type and will update regardless
    }

    /// <inheritdoc />
    public override void ApplyPreview()
    {
        if (_originalGradient == null)
            return;

        // Make sure something actually changed
        if (Equals(ColorGradient, _originalGradient))
            LayerProperty.CurrentValue = _originalGradient;
        else
            // Update the gradient for realsies, giving the command a reference to the old gradient
            ProfileEditorService.ExecuteCommand(new UpdateLayerProperty<ColorGradient>(LayerProperty, ColorGradient, _originalGradient, Time));

        _originalGradient = null;
    }

    /// <inheritdoc />
    public override void DiscardPreview()
    {
        if (_originalGradient == null)
            return;

        // Put the old gradient back
        InputValue = _originalGradient;
        ColorGradient = new ColorGradient(InputValue);
    }

    #endregion
}