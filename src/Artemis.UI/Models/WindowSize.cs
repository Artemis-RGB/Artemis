using System;
using Avalonia;
using Avalonia.Controls;

namespace Artemis.UI.Models;

public class WindowSize
{
    private bool _applying;
    public int Top { get; set; }
    public int Left { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int MaximizedTop { get; set; }
    public int MaximizedLeft { get; set; }
    public double MaximizedWidth { get; set; }
    public double MaximizedHeight { get; set; }
    public bool IsMaximized { get; set; }

    public void ApplyFromWindow(Window window)
    {
        if (_applying)
            return;

        if (double.IsNaN(window.Width) || double.IsNaN(window.Height))
            return;

        IsMaximized = window.WindowState == WindowState.Maximized;
        if (IsMaximized)
        {
            MaximizedTop = window.Position.Y;
            MaximizedLeft = window.Position.X;
            MaximizedHeight = window.Height;
            MaximizedWidth = window.Width;
        }
        else
        {
            Top = window.Position.Y;
            Left = window.Position.X;
            Height = window.Height;
            Width = window.Width;
        }
    }

    public void ApplyToWindow(Window window)
    {
        if (_applying)
            return;

        try
        {
            _applying = true;
            if (IsMaximized)
            {
                window.Position = new PixelPoint(MaximizedLeft, MaximizedTop);
                window.WindowState = WindowState.Maximized;
            }
            else
            {
                window.Position = new PixelPoint(Left, Top);
                window.Height = Height;
                window.Width = Width;
                window.WindowState = WindowState.Normal;
            }
        }
        finally
        {
            _applying = false;
        }
    }
}