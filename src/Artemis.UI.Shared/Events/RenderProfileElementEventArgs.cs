using System;
using Artemis.Core;

namespace Artemis.UI.Shared.Events;

/// <summary>
///     Provides data on profile element related events raised by the profile editor
/// </summary>
public class RenderProfileElementEventArgs : EventArgs
{
    internal RenderProfileElementEventArgs(RenderProfileElement? renderProfileElement)
    {
        RenderProfileElement = renderProfileElement;
    }

    internal RenderProfileElementEventArgs(RenderProfileElement? renderProfileElement, RenderProfileElement? previousRenderProfileElement)
    {
        RenderProfileElement = renderProfileElement;
        PreviousRenderProfileElement = previousRenderProfileElement;
    }

    /// <summary>
    ///     Gets the profile element the event was raised for
    /// </summary>
    public RenderProfileElement? RenderProfileElement { get; }

    /// <summary>
    ///     If applicable, the previous active profile element before the event was raised
    /// </summary>
    public RenderProfileElement? PreviousRenderProfileElement { get; }
}