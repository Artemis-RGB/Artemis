namespace Artemis.Core;

/// <summary>
///     Represents a node that has a custom view model.
/// </summary>
public interface ICustomViewModelNode
{
    /// <summary>
    ///     Gets or sets the position of the node's custom view model.
    /// </summary>
    CustomNodeViewModelPosition ViewModelPosition { get; }

    /// <summary>
    ///     Called whenever the node must show it's custom view model, if <see langword="null" />, no custom view model is used
    /// </summary>
    /// <returns>The custom view model, if <see langword="null" />, no custom view model is used</returns>
    ICustomNodeViewModel? GetCustomViewModel(NodeScript nodeScript);
}