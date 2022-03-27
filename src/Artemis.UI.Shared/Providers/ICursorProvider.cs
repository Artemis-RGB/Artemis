using Avalonia.Input;

namespace Artemis.UI.Shared.Providers;

/// <summary>
///     Represents a provider for custom cursors.
/// </summary>
public interface ICursorProvider
{
    /// <summary>
    ///     A cursor that indicates a rotating.
    /// </summary>
    public Cursor Rotate { get; }

    /// <summary>
    ///     A cursor that indicates dragging.
    /// </summary>
    public Cursor Drag { get; }

    /// <summary>
    ///     A cursor that indicates dragging horizontally.
    /// </summary>
    public Cursor DragHorizontal { get; }
}