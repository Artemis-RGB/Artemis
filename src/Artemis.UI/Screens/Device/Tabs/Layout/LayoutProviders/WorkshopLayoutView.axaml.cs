using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Device.Layout.LayoutProviders;

public partial class WorkshopLayoutView : ReactiveUserControl<WorkshopLayoutViewModel>
{
    public WorkshopLayoutView()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (ViewModel != null && await ViewModel.BrowseLayouts())
            (VisualRoot as Window)?.Close();
    }
}