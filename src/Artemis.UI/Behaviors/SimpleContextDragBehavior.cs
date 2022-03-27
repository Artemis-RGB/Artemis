// Based on AvaloniaBehaviors
// https://github.com/wieslawsoltes/AvaloniaBehaviors/blob/2f972ebb0066a2a4235126da7e103f684de1c777/src/Avalonia.Xaml.Interactions/DragAndDrop/ContextDragBehavior.cs

// The MIT License (MIT)
// 
// Copyright(c) Wiesław Šoltés
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
// 
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactions.DragAndDrop;
using Avalonia.Xaml.Interactivity;

namespace Artemis.UI.Behaviors;

public class SimpleContextDragBehavior : Behavior<Control>
{
    public static readonly StyledProperty<object?> ContextProperty =
        AvaloniaProperty.Register<SimpleContextDragBehavior, object?>(nameof(Context));

    public static readonly StyledProperty<IDragHandler?> HandlerProperty =
        AvaloniaProperty.Register<SimpleContextDragBehavior, IDragHandler?>(nameof(Handler));

    public static readonly StyledProperty<double> HorizontalDragThresholdProperty =
        AvaloniaProperty.Register<SimpleContextDragBehavior, double>(nameof(HorizontalDragThreshold), 3);

    public static readonly StyledProperty<double> VerticalDragThresholdProperty =
        AvaloniaProperty.Register<SimpleContextDragBehavior, double>(nameof(VerticalDragThreshold), 3);

    private Point _dragStartPoint;
    private bool _lock;
    private PointerEventArgs? _triggerEvent;

    public object? Context
    {
        get => GetValue(ContextProperty);
        set => SetValue(ContextProperty, value);
    }

    public IDragHandler? Handler
    {
        get => GetValue(HandlerProperty);
        set => SetValue(HandlerProperty, value);
    }

    public double HorizontalDragThreshold
    {
        get => GetValue(HorizontalDragThresholdProperty);
        set => SetValue(HorizontalDragThresholdProperty, value);
    }

    public double VerticalDragThreshold
    {
        get => GetValue(VerticalDragThresholdProperty);
        set => SetValue(VerticalDragThresholdProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.AddHandler(InputElement.PointerPressedEvent, AssociatedObject_PointerPressed, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject?.AddHandler(InputElement.PointerReleasedEvent, AssociatedObject_PointerReleased, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject?.AddHandler(InputElement.PointerMovedEvent, AssociatedObject_PointerMoved, RoutingStrategies.Direct | RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.RemoveHandler(InputElement.PointerPressedEvent, AssociatedObject_PointerPressed);
        AssociatedObject?.RemoveHandler(InputElement.PointerReleasedEvent, AssociatedObject_PointerReleased);
        AssociatedObject?.RemoveHandler(InputElement.PointerMovedEvent, AssociatedObject_PointerMoved);
    }

    private async Task DoDragDrop(PointerEventArgs triggerEvent, object? value)
    {
        DataObject data = new();
        data.Set(ContextDropBehavior.DataFormat, value!);

        await DragDrop.DoDragDrop(triggerEvent, data, DragDropEffects.Move);
    }

    private void AssociatedObject_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (!properties.IsLeftButtonPressed)
            return;
        if (e.Source is not IControl control || AssociatedObject?.DataContext != control.DataContext)
            return;

        _dragStartPoint = e.GetPosition(null);
        _triggerEvent = e;
        _lock = true;
    }

    private void AssociatedObject_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!Equals(e.Pointer.Captured, AssociatedObject))
            return;

        if (e.InitialPressMouseButton == MouseButton.Left && _triggerEvent is { })
        {
            _triggerEvent = null;
            _lock = false;
        }

        e.Pointer.Capture(null);
    }

    private async void AssociatedObject_PointerMoved(object? sender, PointerEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (!properties.IsLeftButtonPressed)
            return;

        if (_triggerEvent is null)
            return;

        Point point = e.GetPosition(null);
        Point diff = _dragStartPoint - point;
        double horizontalDragThreshold = HorizontalDragThreshold;
        double verticalDragThreshold = VerticalDragThreshold;

        if (!(Math.Abs(diff.X) > horizontalDragThreshold) && !(Math.Abs(diff.Y) > verticalDragThreshold))
            return;

        e.Pointer.Capture(AssociatedObject);

        if (_lock)
            _lock = false;
        else
            return;

        object? context = Context ?? AssociatedObject?.DataContext;

        Handler?.BeforeDragDrop(sender, _triggerEvent, context);
        await DoDragDrop(_triggerEvent, context);
        Handler?.AfterDragDrop(sender, _triggerEvent, context);
    }
}