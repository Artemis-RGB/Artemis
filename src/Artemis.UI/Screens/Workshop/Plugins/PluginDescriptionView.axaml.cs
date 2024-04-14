using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginDescriptionView : ReactiveUserControl<PluginDescriptionViewModel>
{
    public PluginDescriptionView()
    {
        InitializeComponent();
    }
}