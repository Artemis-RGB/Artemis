using Avalonia.Controls;
using Avalonia.Interactivity;
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
        if (ViewModel?.DefaultLayoutPath is null)
            return;

        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(ViewModel.DefaultLayoutPath);
        ViewModel.ShowCopiedNotification();
    }

    private void ImagePathButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel?.Device.Layout?.Image?.LocalPath is null)
            return;

        TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(ViewModel.Device.Layout.Image.LocalPath);
        ViewModel.ShowCopiedNotification();
    }
}