using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device;

public partial class DeviceLayoutTabView : ReactiveUserControl<DeviceLayoutTabViewModel>
{
    public DeviceLayoutTabView()
    {
        InitializeComponent();
    }

    private void LayoutPathButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(ViewModel?.DefaultLayoutPath ?? string.Empty);
        ViewModel.ShowCopiedNotification();
    }

    private void ImagePathButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(ViewModel?.Device?.Layout?.Image?.LocalPath ?? string.Empty);
        ViewModel.ShowCopiedNotification();
    }
}