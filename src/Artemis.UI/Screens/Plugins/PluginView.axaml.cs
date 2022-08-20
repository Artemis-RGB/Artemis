using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Plugins;

public partial class PluginView : ReactiveUserControl<PluginViewModel>
{
    private CheckBox _enabledToggle;

    public PluginView()
    {
        InitializeComponent();
        _enabledToggle = this.Find<CheckBox>("EnabledToggle");
        _enabledToggle.Click += EnabledToggleOnClick;
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