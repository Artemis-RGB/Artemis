using System;
using Artemis.UI.Shared.Providers;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Artemis.UI.Windows.Providers;

public class CursorProvider : ICursorProvider
{
    public CursorProvider()
    {
        Rotate = new Cursor(new Bitmap(AssetLoader.Open(new Uri("avares://Artemis.UI.Windows/Assets/Cursors/aero_rotate.png"))), new PixelPoint(21, 10));
        Drag = new Cursor(new Bitmap(AssetLoader.Open(new Uri("avares://Artemis.UI.Windows/Assets/Cursors/aero_drag.png"))), new PixelPoint(11, 3));
        DragHorizontal = new Cursor(new Bitmap(AssetLoader.Open(new Uri("avares://Artemis.UI.Windows/Assets/Cursors/aero_drag_horizontal.png"))), new PixelPoint(16, 5));
    }

    public Cursor Rotate { get; }
    public Cursor Drag { get; }
    public Cursor DragHorizontal { get; }
}