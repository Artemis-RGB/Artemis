using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public class PluginPrerequisitesInstallDialogView : ReactiveUserControl<PluginPrerequisitesInstallDialogViewModel>
{
    public PluginPrerequisitesInstallDialogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}