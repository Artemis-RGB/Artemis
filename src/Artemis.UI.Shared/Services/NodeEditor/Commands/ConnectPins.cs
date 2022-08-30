using System.Collections.Generic;
using System.Linq;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor.Commands;

/// <summary>
///     Represents a node editor command that can be used to connect two pins.
/// </summary>
public class ConnectPins : INodeEditorCommand
{
    private readonly IPin _output;
    private readonly IPin _input;
    private readonly IPin? _originalConnection;

    /// <summary>
    ///     Creates a new instance of the <see cref="ConnectPins" /> class.
    /// </summary>
    /// <param name="source">The source of the connection.</param>
    /// <param name="target">The target of the connection.</param>
    public ConnectPins(IPin source, IPin target)
    {
        if (source.Direction == PinDirection.Output)
        {
            _output = source;
            _input = target;
        }
        else
        {
            _output = target;
            _input = source;
        }

        _originalConnection = _input.ConnectedTo.FirstOrDefault();
    }

    #region Implementation of INodeEditorCommand

    /// <inheritdoc />
    public string DisplayName => "Connect pins";

    /// <inheritdoc />
    public void Execute()
    {
        _input.DisconnectAll();
        _output.ConnectTo(_input);
    }

    /// <inheritdoc />
    public void Undo()
    {
        _input.DisconnectAll();
        if (_originalConnection != null)
            _input.ConnectTo(_originalConnection);
    }

    #endregion
}