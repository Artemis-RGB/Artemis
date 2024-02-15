using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugin;

public partial class PluginDetailsView : ReactiveUserControl<PluginDetailsViewModel>
{
    public PluginDetailsView()
    {
        InitializeComponent();
    }
}