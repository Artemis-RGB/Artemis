using System;
using Artemis.UI.Screens.Root;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core.ApplicationModel;

namespace Artemis.UI;

public class MainWindow : ReactiveCoreWindow<RootViewModel>
{
    private readonly Panel _rootPanel;
    private readonly ContentControl _sidebarContentControl;

    public MainWindow()
    {
        Opened += OnOpened;
        Activated += OnActivated;
        Deactivated += OnDeactivated;
        InitializeComponent();
        _rootPanel = this.Get<Panel>("RootPanel");
        _sidebarContentControl = this.Get<ContentControl>("SidebarContentControl");
        _rootPanel.LayoutUpdated += OnLayoutUpdated;

#if DEBUG
        this.AttachDevTools();
#endif
    }

    // TODO: Replace with a media query once https://github.com/AvaloniaUI/Avalonia/pull/7938 is implemented
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        _sidebarContentControl.Width = _rootPanel.Bounds.Width >= 1800 ? 300 : 240;
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        Opened -= OnOpened;
        ICoreApplicationView coreAppTitleBar = this;
        if (coreAppTitleBar.TitleBar != null)
        {
            coreAppTitleBar.TitleBar.ExtendViewIntoTitleBar = true;
            SetTitleBar(this.Get<Border>("DragHandle"));
        }
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