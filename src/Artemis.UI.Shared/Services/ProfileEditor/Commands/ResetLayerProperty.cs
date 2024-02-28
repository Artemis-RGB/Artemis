using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to reset a layer property to it's default value.
/// </summary>
public class ResetLayerProperty<T> : IProfileEditorCommand
{
    private readonly bool _keyframesEnabled;
    private readonly LayerProperty<T> _layerProperty;
    private readonly T _originalBaseValue;

    /// <summary>
    ///     Creates a new instance of the <see cref="ResetLayerProperty{T}" /> class.
    /// </summary>
    public ResetLayerProperty(LayerProperty<T> layerProperty)
    {
        if (layerProperty.DefaultValue == null)
            throw new ArtemisSharedUIException("Can't reset a layer property without a default value.");

        _layerProperty = layerProperty;
        _originalBaseValue = _layerProperty.BaseValue;
        _keyframesEnabled = _layerProperty.KeyframesEnabled;
    }

    #region Implementation of IProfileEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Reset layer property";

    /// <inheritdoc />
    public void Execute()
    {
        string json = CoreJson.Serialize(_layerProperty.DefaultValue);

        if (_keyframesEnabled)
            _layerProperty.KeyframesEnabled = false;

        _layerProperty.SetCurrentValue(CoreJson.Deserialize<T>(json)!);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _layerProperty.SetCurrentValue(_originalBaseValue);
        if (_keyframesEnabled)
            _layerProperty.KeyframesEnabled = true;
    }

    #endregion
}