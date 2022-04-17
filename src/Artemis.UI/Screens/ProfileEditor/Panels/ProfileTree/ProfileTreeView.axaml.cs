using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.ReactiveUI;
using Avalonia.VisualTree;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree;

public class ProfileTreeView : ReactiveUserControl<ProfileTreeViewModel>
{
    private Image? _dragAdorner;
    private Point _dragStartPosition;
    private Point _elementDragOffset;
    private TreeView _treeView;

    public ProfileTreeView()
    {
        InitializeComponent();
        AddHandler(DragDrop.DragEnterEvent, HandleDragEnterEvent, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        AddHandler(DragDrop.DragOverEvent, HandleDragOver, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
        AddHandler(PointerEnterEvent, HandlePointerEnter, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble, true);
    }

    private void HandlePointerEnter(object? sender, PointerEventArgs e)
    {
        DisposeDragAdorner();
    }

    private void HandleDragEnterEvent(object? sender, DragEventArgs e)
    {
        CreateDragAdorner(e);
    }

    private void CreateDragAdorner(DragEventArgs e)
    {
        if (_dragAdorner != null)
            return;

        if (e.Source is not Control c)
            return;

        // Get the tree view item that raised the event
        TreeViewItem? container = c.FindLogicalAncestorOfType<TreeViewItem>();
        if (container == null)
            return;

        // Take a snapshot of said tree view item and add it as an adorner
        ITransform? originalTransform = container.RenderTransform;
        try
        {
            _dragStartPosition = e.GetPosition(this.FindAncestorOfType<Window>());
            _elementDragOffset = e.GetPosition(container);

            RenderTargetBitmap renderTarget = new(new PixelSize((int) container.Bounds.Width, (int) container.Bounds.Height));
            container.RenderTransform = new TranslateTransform(container.Bounds.X * -1, container.Bounds.Y * -1);
            renderTarget.Render(container);
            _dragAdorner = new Image
            {
                Source = renderTarget,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Stretch = Stretch.None,
                IsHitTestVisible = false
            };
            AdornerLayer.GetAdornerLayer(this)!.Children.Add(_dragAdorner);
        }
        finally
        {
            container.RenderTransform = originalTransform;
        }
    }

    private void HandleDragOver(object? sender, DragEventArgs e)
    {
        UpdateDragAdorner(e);
    }

    private void HandleLeaveEvent(object? sender, RoutedEventArgs e)
    {
        // If there is currently an adorner, dispose of it
        DisposeDragAdorner();
    }

    private void HandleDrop(object? sender, DragEventArgs e)
    {
        // If there is currently an adorner, dispose of it
        DisposeDragAdorner();
    }

    private void DisposeDragAdorner()
    {
        if (_dragAdorner == null)
            return;

        AdornerLayer.GetAdornerLayer(this)!.Children.Remove(_dragAdorner);
        (_dragAdorner.Source as RenderTargetBitmap)?.Dispose();
        _dragAdorner = null;
    }

    private void UpdateDragAdorner(DragEventArgs e)
    {
        if (_dragAdorner == null)
            return;

        Point position = e.GetPosition(this.FindAncestorOfType<Window>());
        _dragAdorner.RenderTransform = new TranslateTransform(_dragStartPosition.X - _elementDragOffset.X, position.Y - _elementDragOffset.Y);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _treeView = this.Get<TreeView>("ProfileTreeView");
    }

    private void ProfileTreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _treeView.Focus();
    }
}