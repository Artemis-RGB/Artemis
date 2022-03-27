using System.Collections.Generic;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to remove a pin from a pin collection.
/// </summary>
public class RemovePin : INodeEditorCommand
{
    private readonly IPinCollection _pinCollection;
    private readonly IPin _pin;
    private readonly List<IPin> _originalConnections;

    /// <summary>
    ///     Creates a new instance of the <see cref="RemovePin" /> class.
    /// </summary>
    /// <param name="pinCollection">The pin collection to add the pin to.</param>
    /// <param name="pin">The pin to remove.</param>
    public RemovePin(IPinCollection pinCollection, IPin pin)
    {
        if (!pinCollection.Contains(pin))
            throw new ArtemisSharedUIException("Can't remove a pin from a collection it isn't contained in.");

        _pinCollection = pinCollection;
        _pin = pin;

        _originalConnections = new List<IPin>(_pin.ConnectedTo);
    }

    /// <inheritdoc />
    public string DisplayName => "Remove pin";

    /// <inheritdoc />
    public void Execute()
    {
        _pin.DisconnectAll();
        _pinCollection.Remove(_pin);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _pinCollection.Add(_pin);
        foreach (IPin originalConnection in _originalConnections)
            _pin.ConnectTo(originalConnection);
    }
}