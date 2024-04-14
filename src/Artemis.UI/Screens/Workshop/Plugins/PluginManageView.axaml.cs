using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginManageView : ReactiveUserControl<PluginManageViewModel>
{
    public PluginManageView()
    {
        InitializeComponent();
    }
}