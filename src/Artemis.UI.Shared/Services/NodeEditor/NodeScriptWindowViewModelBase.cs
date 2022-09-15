using Artemis.Core;

namespace Artemis.UI.Shared.Services.NodeEditor;

/// <summary>
///     Represents the base of the node script editor window view model.
/// </summary>
public abstract class NodeScriptWindowViewModelBase : DialogViewModelBase<bool>
{
    /// <summary>
    ///     Creates a new instance of the <see cref="NodeScriptWindowViewModelBase" /> class.
    /// </summary>
    /// <param name="nodeScript">The node script being edited.</param>
    protected NodeScriptWindowViewModelBase(NodeScript nodeScript)
    {
        NodeScript = nodeScript;
    }

    /// <summary>
    ///     Gets the node script being edited.
    /// </summary>
    public NodeScript NodeScript { get; init; }
}