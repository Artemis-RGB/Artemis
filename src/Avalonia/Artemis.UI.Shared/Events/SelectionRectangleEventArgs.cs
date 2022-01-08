using System;
using Artemis.UI.Shared.Controls;
using Avalonia;
using Avalonia.Input;

namespace Artemis.UI.Shared.Events
{
    /// <summary>
    ///     Provides data on selection events raised by the <see cref="SelectionRectangle" />.
    /// </summary>
    public class SelectionRectangleEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="SelectionRectangleEventArgs" /> class.
        /// </summary>
        public SelectionRectangleEventArgs(Rect rectangle, KeyModifiers keyModifiers)
        {
            KeyModifiers = keyModifiers;
            Rectangle = rectangle;
        }

        /// <summary>
        ///     Gets the rectangle that was selected when the event occurred.
        /// </summary>
        public Rect Rectangle { get; }

        /// <summary>
        ///     Gets the key modifiers that where pressed when the event occurred.
        /// </summary>
        public KeyModifiers KeyModifiers { get; }
    }
}