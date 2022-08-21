using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to connect two pins.
/// </summary>
public class ConnectPins : INodeEditorCommand
{
    private readonly List<IPin>? _originalConnections;
    private readonly IPin _source;
    private readonly IPin _target;

    /// <summary>
    ///     Creates a new instance of the <see cref="ConnectPins" /> class.
    /// </summary>
    /// <param name="source">The source of the connection.</param>
    /// <param name="target">The target of the connection.</param>
    public ConnectPins(IPin source, IPin target)
    {
        _source = source;
        _target = target;

        _originalConnections = _target.Direction == PinDirection.Input ? new List<IPin>(_target.ConnectedTo) : null;
    }

    #region Implementation of INodeEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Connect pins";

    /// <inheritdoc />
    public void Execute()
    {
        if (_target.Direction == PinDirection.Input)
            _target.DisconnectAll();
        _source.ConnectTo(_target);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _target.DisconnectFrom(_source);

        if (_originalConnections == null)
            return;
        foreach (IPin pin in _originalConnections)
            _target.ConnectTo(pin);
    }

    #endregion
}