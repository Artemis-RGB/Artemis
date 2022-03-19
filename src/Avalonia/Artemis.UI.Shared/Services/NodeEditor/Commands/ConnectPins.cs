using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;

public class ConnectPins : INodeEditorCommand
{
    private readonly IPin _source;
    private readonly IPin _target;
    private readonly List<IPin>? _originalConnections;

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