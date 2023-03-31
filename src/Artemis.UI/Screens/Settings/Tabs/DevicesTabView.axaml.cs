using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class DevicesTabView : ReactiveUserControl<DevicesTabViewModel>
{
    public DevicesTabView()
    {
        InitializeComponent();
    }

}