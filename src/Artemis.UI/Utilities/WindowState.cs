using System;
using System.Windows;
using MaterialDesignExtensions.Controls;

namespace Artemis.UI.Utilities
{
    public class WindowSize
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsMaximized { get; set; }

        public void ApplyFromWindow(MaterialWindow window)
        {
            Top = window.Top;
            Left = window.Left;
            Height = window.Height;
            Width = window.Width;
            IsMaximized = window.WindowState == WindowState.Maximized;
        }

        public void ApplyToWindow(MaterialWindow window)
        {
            window.Top = Math.Max(0, Top);
            window.Left = Math.Max(0, Left);
            window.Height = Height;
            window.Width = Width;
            window.WindowState = IsMaximized ? WindowState.Maximized : WindowState.Normal;
        }
    }
}