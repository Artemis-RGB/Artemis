using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class PluginsTabView : ReactiveUserControl<PluginsTabViewModel>
{
    public PluginsTabView()
    {
        InitializeComponent();
    }
}