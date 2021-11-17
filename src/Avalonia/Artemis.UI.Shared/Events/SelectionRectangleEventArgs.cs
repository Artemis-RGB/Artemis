using System;
using Avalonia;

namespace Artemis.UI.Shared.Events
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