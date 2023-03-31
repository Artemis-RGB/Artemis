using System;
using System.Reactive;
using System.Reactive.Linq;
using Artemis.UI.Models;
using Artemis.UI.Screens.Root;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI;

public partial class MainWindow : ReactiveAppWindow<RootViewModel>
{
    private readonly Panel _rootPanel;
    private readonly ContentControl _sidebarContentControl;
    private bool _activated;

    public MainWindow()
    {
        Opened += OnOpened;
        Activated += OnActivated;
        Deactivated += OnDeactivated;

        // ApplyWindowSize();
        InitializeComponent();

        _rootPanel = this.Get<Panel>("RootPanel");
        _sidebarContentControl = this.Get<ContentControl>("SidebarContentControl");
        _rootPanel.LayoutUpdated += OnLayoutUpdated;

#if DEBUG
        this.AttachDevTools();
#endif

        Observable.FromEventPattern<PixelPointEventArgs>(x => PositionChanged += x, x => PositionChanged -= x)
            .Select(_ => Unit.Default)
            .Merge(this.WhenAnyValue(vm => vm.WindowState, vm => vm.Width, vm => vm.Width, vm => vm.Height).Select(_ => Unit.Default))
            .Throttle(TimeSpan.FromMilliseconds(200), AvaloniaScheduler.Instance)
            .Subscribe(_ => SaveWindowSize());
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

    // TODO: Replace with a media query once https://github.com/AvaloniaUI/Avalonia/pull/7938 is implemented
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        _sidebarContentControl.Width = _rootPanel.Bounds.Width >= 1800 ? 300 : 240;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;
        // ICoreApplicationView coreAppTitleBar = this;
        // if (coreAppTitleBar.TitleBar != null)
        // {
        //     coreAppTitleBar.TitleBar.ExtendViewIntoTitleBar = true;
        //     SetTitleBar(this.Get<Border>("DragHandle"));
        // }
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        ViewModel?.Focused();
    }

    private void OnDeactivated(object? sender, EventArgs e)
    {
        ViewModel?.Unfocused();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}