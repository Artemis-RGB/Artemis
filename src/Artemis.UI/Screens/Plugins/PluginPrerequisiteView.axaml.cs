using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginPrerequisiteView : ReactiveUserControl<PluginPrerequisiteViewModel>
{
    public PluginPrerequisiteView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}