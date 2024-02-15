using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Tabs;

public partial class PluginListView : ReactiveUserControl<PluginListViewModel>
{
    public PluginListView()
    {
        InitializeComponent();
    }
}