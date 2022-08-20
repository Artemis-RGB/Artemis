using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to add a pin to a pin collection.
/// </summary>
public class AddPin : INodeEditorCommand
{
    private readonly IPinCollection _pinCollection;
    private IPin? _pin;

    /// <summary>
    ///     Creates a new instance of the <see cref="AddPin" /> class.
    /// </summary>
    /// <param name="pinCollection">The pin collection to add the pin to.</param>
    public AddPin(IPinCollection pinCollection)
    {
        _pinCollection = pinCollection;
    }

    /// <inheritdoc />
    public string DisplayName => "Add pin";

    /// <inheritdoc />
    public void Execute()
    {
        _pin ??= _pinCollection.CreatePin();
        _pinCollection.Add(_pin);
    }

    /// <inheritdoc />
    public void Undo()
    {
        if (_pin != null)
            _pinCollection.Remove(_pin);
    }
}