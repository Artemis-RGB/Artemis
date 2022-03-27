using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Transformation;
using Avalonia.Xaml.Interactivity;

namespace Artemis.UI.Behaviors;

/// <summary>
/// </summary>
public class TreeItemDragBehavior : Behavior<IControl>
{
    /// <summary>
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<TreeItemDragBehavior, Orientation>(nameof(Orientation));

    /// <summary>
    /// </summary>
    public static readonly StyledProperty<double> HorizontalDragThresholdProperty =
        AvaloniaProperty.Register<TreeItemDragBehavior, double>(nameof(HorizontalDragThreshold), 3);

    /// <summary>
    /// </summary>
    public static readonly StyledProperty<double> VerticalDragThresholdProperty =
        AvaloniaProperty.Register<TreeItemDragBehavior, double>(nameof(VerticalDragThreshold), 3);

    private IControl? _draggedContainer;
    private int _draggedIndex;
    private bool _dragStarted;
    private bool _enableDrag;
    private ItemsControl? _itemsControl;
    private Point _start;
    private int _targetIndex;

    /// <summary>
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// </summary>
    public double HorizontalDragThreshold
    {
        get => GetValue(HorizontalDragThresholdProperty);
        set => SetValue(HorizontalDragThresholdProperty, value);
    }

    /// <summary>
    /// </summary>
    public double VerticalDragThreshold
    {
        get => GetValue(VerticalDragThresholdProperty);
        set => SetValue(VerticalDragThresholdProperty, value);
    }

    /// <summary>
    /// </summary>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { })
        {
            AssociatedObject.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
            AssociatedObject.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel);
        }
    }

    /// <summary>
    /// </summary>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is { })
        {
            AssociatedObject.RemoveHandler(InputElement.PointerReleasedEvent, Released);
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
            AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            AssociatedObject.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
        }
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed
            && AssociatedObject?.Parent is ItemsControl itemsControl)
        {
            _enableDrag = true;
            _dragStarted = false;
            _start = e.GetPosition(AssociatedObject.Parent);
            _draggedIndex = -1;
            _targetIndex = -1;
            _itemsControl = itemsControl;
            _draggedContainer = AssociatedObject;

            if (_draggedContainer is { })
                SetDraggingPseudoClasses(_draggedContainer, true);

            AddTransforms(_itemsControl);
        }
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragStarted)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
                Released();

            e.Pointer.Capture(null);
        }
    }

    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        Released();
    }

    private void Released()
    {
        if (!_enableDrag)
            return;

        RemoveTransforms(_itemsControl);

        if (_itemsControl is { })
            foreach (ItemContainerInfo? container in _itemsControl.ItemContainerGenerator.Containers)
                SetDraggingPseudoClasses(container.ContainerControl, true);

        if (_dragStarted)
            if (_draggedIndex >= 0 && _targetIndex >= 0 && _draggedIndex != _targetIndex)
                MoveDraggedItem(_itemsControl, _draggedIndex, _targetIndex);

        if (_itemsControl is { })
            foreach (ItemContainerInfo? container in _itemsControl.ItemContainerGenerator.Containers)
                SetDraggingPseudoClasses(container.ContainerControl, false);

        if (_draggedContainer is { })
            SetDraggingPseudoClasses(_draggedContainer, false);

        _draggedIndex = -1;
        _targetIndex = -1;
        _enableDrag = false;
        _dragStarted = false;
        _itemsControl = null;

        _draggedContainer = null;
    }

    private void AddTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null)
            return;

        int i = 0;

        foreach (object? _ in itemsControl.Items)
        {
            IControl? container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null)
                SetTranslateTransform(container, 0, 0);

            i++;
        }
    }

    private void RemoveTransforms(ItemsControl? itemsControl)
    {
        if (itemsControl?.Items is null)
            return;

        int i = 0;

        foreach (object? _ in itemsControl.Items)
        {
            IControl? container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
            if (container is not null)
                SetTranslateTransform(container, 0, 0);

            i++;
        }
    }

    private void MoveDraggedItem(ItemsControl? itemsControl, int draggedIndex, int targetIndex)
    {
        if (itemsControl?.Items is not IList items)
            return;

        object? draggedItem = items[draggedIndex];
        items.RemoveAt(draggedIndex);
        items.Insert(targetIndex, draggedItem);

        if (itemsControl is SelectingItemsControl selectingItemsControl)
            selectingItemsControl.SelectedIndex = targetIndex;
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (properties.IsLeftButtonPressed)
        {
            if (_itemsControl?.Items is null || _draggedContainer?.RenderTransform is null || !_enableDrag)
                return;

            Orientation orientation = Orientation;
            Point position = e.GetPosition(_itemsControl);
            double delta = orientation == Orientation.Horizontal ? position.X - _start.X : position.Y - _start.Y;

            if (!_dragStarted)
            {
                Point diff = _start - position;
                double horizontalDragThreshold = HorizontalDragThreshold;
                double verticalDragThreshold = VerticalDragThreshold;

                if (orientation == Orientation.Horizontal)
                {
                    if (Math.Abs(diff.X) > horizontalDragThreshold)
                        _dragStarted = true;
                    else
                        return;
                }
                else
                {
                    if (Math.Abs(diff.Y) > verticalDragThreshold)
                        _dragStarted = true;
                    else
                        return;
                }
                e.Pointer.Capture(AssociatedObject);
            }

            if (orientation == Orientation.Horizontal)
                SetTranslateTransform(_draggedContainer, delta, 0);
            else
                SetTranslateTransform(_draggedContainer, 0, delta);

            _draggedIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(_draggedContainer);
            _targetIndex = -1;

            Rect draggedBounds = _draggedContainer.Bounds;

            double draggedStart = orientation == Orientation.Horizontal ? draggedBounds.X : draggedBounds.Y;

            double draggedDeltaStart = orientation == Orientation.Horizontal
                ? draggedBounds.X + delta
                : draggedBounds.Y + delta;

            double draggedDeltaEnd = orientation == Orientation.Horizontal
                ? draggedBounds.X + delta + draggedBounds.Width
                : draggedBounds.Y + delta + draggedBounds.Height;

            int i = 0;

            foreach (object? _ in _itemsControl.Items)
            {
                IControl? targetContainer = _itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (targetContainer?.RenderTransform is null || ReferenceEquals(targetContainer, _draggedContainer))
                {
                    i++;
                    continue;
                }

                // If the target container has children, there are two options
                // Move into the top of the container
                // Insert before or after a child in the container

                Rect targetBounds = targetContainer.Bounds;
                double targetStart = orientation == Orientation.Horizontal ? targetBounds.X : targetBounds.Y;

                double targetMid = orientation == Orientation.Horizontal
                    ? targetBounds.X + targetBounds.Width / 2
                    : targetBounds.Y + targetBounds.Height / 2;

                int targetIndex = _itemsControl.ItemContainerGenerator.IndexFromContainer(targetContainer);

                if (targetStart > draggedStart && draggedDeltaEnd >= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                        SetTranslateTransform(targetContainer, -draggedBounds.Width, 0);
                    else
                        SetTranslateTransform(targetContainer, 0, -draggedBounds.Height);

                    _targetIndex = _targetIndex == -1 ? targetIndex :
                        targetIndex > _targetIndex ? targetIndex : _targetIndex;
                }
                else if (targetStart < draggedStart && draggedDeltaStart <= targetMid)
                {
                    if (orientation == Orientation.Horizontal)
                        SetTranslateTransform(targetContainer, draggedBounds.Width, 0);
                    else
                        SetTranslateTransform(targetContainer, 0, draggedBounds.Height);

                    _targetIndex = _targetIndex == -1 ? targetIndex :
                        targetIndex < _targetIndex ? targetIndex : _targetIndex;
                }
                else
                {
                    if (orientation == Orientation.Horizontal)
                        SetTranslateTransform(targetContainer, 0, 0);
                    else
                        SetTranslateTransform(targetContainer, 0, 0);
                }

                i++;
            }
        }
    }

    private void SetDraggingPseudoClasses(IControl control, bool isDragging)
    {
        if (isDragging)
            ((IPseudoClasses) control.Classes).Add(":dragging");
        else
            ((IPseudoClasses) control.Classes).Remove(":dragging");
    }

    private void SetTranslateTransform(IControl control, double x, double y)
    {
        TransformOperations.Builder transformBuilder = new(1);
        transformBuilder.AppendTranslate(x, y);
        control.RenderTransform = transformBuilder.Build();
    }
}