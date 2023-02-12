using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings.Updating;

public partial class ReleaseAvailableView : ReactiveCoreWindow<ReleaseAvailableViewModel>
{
    public ReleaseAvailableView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}