using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor;

/// <summary>
///     Represents a scope in which editor commands are executed until disposed.
/// </summary>
public class NodeEditorCommandScope : IDisposable
{
    private readonly List<INodeEditorCommand> _commands;

    private readonly NodeEditorService _nodeEditorService;
    private readonly INodeScript _nodeScript;

    internal NodeEditorCommandScope(NodeEditorService nodeEditorService, INodeScript nodeScript, string name)
    {
        Name = name;
        _nodeEditorService = nodeEditorService;
        _nodeScript = nodeScript;
        _commands = [];
    }

    /// <summary>
    ///     Gets the name of the scope.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets a read only collection of commands in the scope.
    /// </summary>
    public ReadOnlyCollection<INodeEditorCommand> NodeEditorCommands => new(_commands);

    internal void AddCommand(INodeEditorCommand command)
    {
        command.Execute();
        _commands.Add(command);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _nodeEditorService.StopCommandScope(_nodeScript);
    }
}