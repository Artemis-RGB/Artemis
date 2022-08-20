using System;
using Artemis.Core;
using Artemis.UI.Shared.Services.ProfileEditor;

namespace Artemis.UI.Shared.Services.NodeEditor;

/// <summary>
///     Represents a service that can be used to execute editor commands on node scripts.
/// </summary>
public interface INodeEditorService : IArtemisSharedUIService
{
    /// <summary>
    ///     Gets the editor history for the provided node script.
    /// </summary>
    /// <param name="nodeScript">The node script to get the editor history for.</param>
    /// <returns>The node editor history of the given node script.</returns>
    NodeEditorHistory GetHistory(INodeScript nodeScript);

    /// <summary>
    ///     Executes the provided command and adds it to the history.
    /// </summary>
    /// <param name="nodeScript">The node script to execute the command upon.</param>
    /// <param name="command">The command to execute.</param>
    void ExecuteCommand(INodeScript nodeScript, INodeEditorCommand command);

    /// <summary>
    ///     Creates a new command scope which can be used to group undo/redo actions of multiple commands.
    /// </summary>
    /// <param name="nodeScript">The node script to create the scope for.</param>
    /// <param name="name">The name of the command scope.</param>
    /// <returns>The command scope that will group any commands until disposed.</returns>
    NodeEditorCommandScope CreateCommandScope(INodeScript nodeScript, string name);
}