using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.NodeEditor.Commands;

namespace Artemis.UI.Shared.Services.NodeEditor;

/// <inheritdoc cref="INodeEditorService"/>
public class NodeEditorService : INodeEditorService
{
    private readonly IWindowService _windowService;

    public NodeEditorService(IWindowService windowService)
    {
        _windowService = windowService;
    }

    private readonly Dictionary<INodeScript, NodeEditorHistory> _nodeEditorHistories = new();
    private readonly Dictionary<INodeScript, NodeEditorCommandScope> _nodeEditorCommandScopes = new();

    /// <inheritdoc />
    public NodeEditorHistory GetHistory(INodeScript nodeScript)
    {
        if (_nodeEditorHistories.TryGetValue(nodeScript, out NodeEditorHistory? history))
            return history;

        NodeEditorHistory newHistory = new(nodeScript);
        _nodeEditorHistories.Add(nodeScript, newHistory);
        return newHistory;
    }

    /// <inheritdoc />
    public void ExecuteCommand(INodeScript nodeScript, INodeEditorCommand command)
    {
        try
        {
            NodeEditorHistory history = GetHistory(nodeScript);

            // If a scope is active add the command to it, the scope will execute it immediately
            _nodeEditorCommandScopes.TryGetValue(nodeScript, out NodeEditorCommandScope? scope);
            if (scope != null)
            {
                scope.AddCommand(command);
                return;
            }

            history.Execute.Execute(command).Subscribe();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Editor command failed", e);
            throw;
        }
    }

    /// <inheritdoc />
    public NodeEditorCommandScope CreateCommandScope(INodeScript nodeScript, string name)
    {
        if (_nodeEditorCommandScopes.TryGetValue(nodeScript, out NodeEditorCommandScope? scope))
            throw new ArtemisSharedUIException($"A command scope is already active, name: {scope.Name}.");

        NodeEditorCommandScope newScope = new(this, nodeScript, name);
        _nodeEditorCommandScopes.Add(nodeScript, newScope);
        return newScope;
    }

    internal void StopCommandScope(INodeScript nodeScript)
    {
        // This might happen if the scope is disposed twice, it's no biggie
        if (!_nodeEditorCommandScopes.TryGetValue(nodeScript, out NodeEditorCommandScope? scope))
            return;

        _nodeEditorCommandScopes.Remove(nodeScript);

        // Executing the composite command won't do anything the first time (see last ctor variable)
        // commands were already executed each time they were added to the scope
        ExecuteCommand(nodeScript, new CompositeCommand(scope.NodeEditorCommands, scope.Name, true));
    }
}