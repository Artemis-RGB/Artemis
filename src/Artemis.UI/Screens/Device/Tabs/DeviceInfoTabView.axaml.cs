using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device;

public partial class DeviceInfoTabView : ReactiveUserControl<DeviceInfoTabViewModel>
{
    public DeviceInfoTabView()
    {
        InitializeComponent();
    }

    private void LayoutPathButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(ViewModel.DefaultLayoutPath);
    }

    private void ImagePathButton_OnClick(object? sender, RoutedEventArgs e)
    {
        TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(ViewModel.Device.Layout.Image.LocalPath);
    }
}