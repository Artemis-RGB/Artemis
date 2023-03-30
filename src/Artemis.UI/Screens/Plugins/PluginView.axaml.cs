using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Plugins;

public partial class PluginView : ReactiveUserControl<PluginViewModel>
{
    public PluginView()
    {
        InitializeComponent();
        EnabledToggle.Click += EnabledToggleOnClick;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void EnabledToggleOnClick(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => ViewModel?.UpdateEnabled(!ViewModel.Plugin.IsEnabled));
    }

    private void FlyoutBase_OnOpening(object? sender, EventArgs e)
    {
        ViewModel?.CheckPrerequisites();
    }
}