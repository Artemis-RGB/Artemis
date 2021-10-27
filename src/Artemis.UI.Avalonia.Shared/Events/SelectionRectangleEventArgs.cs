using System;
using Avalonia;

namespace Artemis.UI.Avalonia.Shared.Events
{
    public class SelectionRectangleEventArgs : EventArgs
    {
        public SelectionRectangleEventArgs(Rect rect)
        {
            Rect = rect;
        }

        public Rect Rect { get; }
    }
}