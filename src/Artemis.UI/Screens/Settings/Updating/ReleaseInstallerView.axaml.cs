using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Settings.Updating;

public partial class ReleaseInstallerView : ReactiveCoreWindow<ReleaseInstallerViewModel>
{
    public ReleaseInstallerView()
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

    private void Cancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}