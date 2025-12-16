using System;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.UI.Models;
using Artemis.UI.Screens.Root;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI;

public partial class MainWindow : ReactiveAppWindow<RootViewModel>
{
    private bool _activated;
    private IDisposable? _positionObserver;

    public MainWindow()
    {
        Opened += OnOpened;
        Closed += OnClosed;
        Activated += OnActivated;
        Deactivated += OnDeactivated;

        InitializeComponent();
        ApplyWindowSize();

        Shared.UI.Clipboard = GetTopLevel(this)!.Clipboard!;
        RootPanel.LayoutUpdated += OnLayoutUpdated;

#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void ApplyWindowSize()
    {
        _activated = true;
        RootViewModel.WindowSizeSetting?.Value?.ApplyToWindow(this);
    }

    private void SaveWindowSize()
    {
        if (RootViewModel.WindowSizeSetting == null || !_activated)
            return;

        RootViewModel.WindowSizeSetting.Value ??= new WindowSize();
        RootViewModel.WindowSizeSetting.Value.ApplyFromWindow(this);
    }

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        SidebarContentControl.Width = RootPanel.Bounds.Width >= 1800 ? 300 : 240;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        TitleBar.ExtendsContentIntoTitleBar = true;

        _positionObserver = Observable.FromEventPattern<PixelPointEventArgs>(x => PositionChanged += x, x => PositionChanged -= x)
            .Select(_ => Unit.Default)
            .Merge(this.WhenAnyValue(vm => vm.WindowState, vm => vm.Width, vm => vm.Width, vm => vm.Height).Select(_ => Unit.Default))
            .Throttle(TimeSpan.FromMilliseconds(200), AvaloniaScheduler.Instance)
            .Subscribe(_ => SaveWindowSize());
    }
    
    private void OnClosed(object? sender, EventArgs e)
    {
        _positionObserver?.Dispose();
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        ViewModel?.Focused();
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        ViewModel?.Unfocused();
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.XButton1)
            ViewModel?.GoBack();
        else if (e.InitialPressMouseButton == MouseButton.XButton2)
            ViewModel?.GoForward();
    }
}