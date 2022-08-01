using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to connect two pins.
/// </summary>
public class DisconnectPins : INodeEditorCommand
{
    private readonly IPin _pin;
    private readonly List<IPin> _originalConnections;

    /// <summary>
    ///     Creates a new instance of the <see cref="DisconnectPins" /> class.
    /// </summary>
    /// <param name="pin">The pin to disconnect.</param>
    public DisconnectPins(IPin pin)
    {
        _pin = pin;
        _originalConnections = new List<IPin>(_pin.ConnectedTo);
    }

    #region Implementation of INodeEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Disconnect pins";

    /// <inheritdoc />
    public void Execute()
    {
        _pin.DisconnectAll();
    }

    /// <inheritdoc />
    public void Undo()
    {
        foreach (IPin pin in _originalConnections)
            _pin.ConnectTo(pin);
    }

    #endregion
}