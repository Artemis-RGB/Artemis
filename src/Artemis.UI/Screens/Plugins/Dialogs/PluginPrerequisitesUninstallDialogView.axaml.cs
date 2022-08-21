using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginPrerequisitesUninstallDialogView : ReactiveUserControl<PluginPrerequisitesUninstallDialogViewModel>
{
    public PluginPrerequisitesUninstallDialogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}