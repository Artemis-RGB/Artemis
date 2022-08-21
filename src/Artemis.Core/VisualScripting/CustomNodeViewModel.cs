using System;

namespace Artemis.Core;

/// <summary>
///     Represents a custom view model for a <see cref="INode" />
/// </summary>
public interface ICustomNodeViewModel
{
    /// <summary>
    ///     Gets the node the view models belongs to
    /// </summary>
    public INode Node { get; }

    /// <summary>
    ///     Occurs whenever the node was modified by the view model
    /// </summary>
    event EventHandler NodeModified;
}