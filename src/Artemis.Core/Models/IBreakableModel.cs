using System;
using System.Collections.Generic;

namespace Artemis.Core;

/// <summary>
///     Represents a model that can have a broken state
/// </summary>
public interface IBreakableModel
{
    /// <summary>
    ///     Gets the display name of this breakable model
    /// </summary>
    string BrokenDisplayName { get; }

    /// <summary>
    ///     Gets or sets the broken state of this breakable model, if <see langword="null" /> this model is not broken.
    /// </summary>
    string? BrokenState { get; set; }

    /// <summary>
    ///     Gets or sets the exception that caused the broken state
    /// </summary>
    Exception? BrokenStateException { get; set; }

    /// <summary>
    ///     Try to execute the provided action. If the action succeeded the broken state is cleared if it matches
    ///     <see paramref="breakMessage" />, if the action throws an exception <see cref="BrokenState" /> and
    ///     <see cref="BrokenStateException" /> are set accordingly.
    /// </summary>
    /// <param name="action">The action to attempt to execute</param>
    /// <param name="breakMessage">The message to clear on succeed or set on failure (exception)</param>
    /// <returns><see langword="true" /> if the action succeeded; otherwise <see langword="false" />.</returns>
    bool TryOrBreak(Action action, string breakMessage);

    /// <summary>
    ///     Sets the broken state to the provided state and optional exception.
    /// </summary>
    /// <param name="state">The state to set the broken state to</param>
    /// <param name="exception">The exception that caused the broken state</param>
    public void SetBrokenState(string state, Exception? exception);

    /// <summary>
    ///     Clears the broken state and exception if <see cref="BrokenState" /> equals <see paramref="state"></see>.
    /// </summary>
    /// <param name="state"></param>
    public void ClearBrokenState(string state);

    /// <summary>
    ///     Returns a list containing all broken models, including self and any children
    /// </summary>
    IEnumerable<IBreakableModel> GetBrokenHierarchy();

    /// <summary>
    ///     Occurs when the broken state of this model changes
    /// </summary>
    event EventHandler BrokenStateChanged;
}