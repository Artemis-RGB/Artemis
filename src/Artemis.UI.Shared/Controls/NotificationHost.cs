using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Artemis.UI.Shared;

internal class NotificationHost : ContentControl
{
    private IDisposable? _rootBoundsWatcher;

    public NotificationHost()
    {
        Background = null;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
    }

    protected override Type StyleKeyOverride => typeof(OverlayPopupHost);

    protected override Size MeasureOverride(Size availableSize)
    {
        _ = base.MeasureOverride(availableSize);

        if (VisualRoot is TopLevel tl)
            return tl.ClientSize;
        if (VisualRoot is Control c)
            return c.Bounds.Size;

        return default;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (e.Root is Control wb)
            // OverlayLayer is a Canvas, so we won't get a signal to resize if the window
            // bounds change. Subscribe to force update
            _rootBoundsWatcher = wb.GetObservable(BoundsProperty).Subscribe(_ => OnRootBoundsChanged());
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _rootBoundsWatcher?.Dispose();
        _rootBoundsWatcher = null;
    }

    private void OnRootBoundsChanged()
    {
        InvalidateMeasure();
    }
}