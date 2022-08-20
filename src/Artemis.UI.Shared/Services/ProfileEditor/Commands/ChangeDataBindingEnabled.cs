using Artemis.Core;

namespace Artemis.UI.Shared.Services.ProfileEditor.Commands;

/// <summary>
///     Represents a profile editor command that can be used to enable or disable data bindings on a layer property.
/// </summary>
public class ChangeDataBindingEnabled : IProfileEditorCommand
{
    private readonly bool _enabled;
    private readonly ILayerProperty _layerProperty;
    private readonly bool _originalEnabled;

    /// <summary>
    ///     Creates a new instance of the <see cref="ChangeDataBindingEnabled" /> class.
    /// </summary>
    /// <param name="layerProperty">The layer property to enable or disable data bindings on.</param>
    /// <param name="enabled">Whether to enable or disable data bindings.</param>
    public ChangeDataBindingEnabled(ILayerProperty layerProperty, bool enabled)
    {
        _layerProperty = layerProperty;
        _enabled = enabled;
        _originalEnabled = _layerProperty.BaseDataBinding.IsEnabled;
    }

    /// <inheritdoc />
    public string DisplayName => _enabled ? "Enable data binding" : "Disable data binding";

    /// <inheritdoc />
    public void Execute()
    {
        _layerProperty.BaseDataBinding.IsEnabled = _enabled;
    }

    /// <inheritdoc />
    public void Undo()
    {
        _layerProperty.BaseDataBinding.IsEnabled = _originalEnabled;
    }
}