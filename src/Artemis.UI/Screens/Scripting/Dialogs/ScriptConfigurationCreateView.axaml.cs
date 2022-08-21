using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Scripting.Dialogs;

public class ScriptConfigurationCreateView : ReactiveUserControl<ScriptConfigurationCreateViewModel>
{
    public ScriptConfigurationCreateView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}