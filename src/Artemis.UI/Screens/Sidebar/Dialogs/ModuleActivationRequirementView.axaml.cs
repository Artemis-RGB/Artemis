using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class ModuleActivationRequirementView : ReactiveUserControl<ModuleActivationRequirementViewModel>
{
    public ModuleActivationRequirementView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}