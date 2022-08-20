using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition;

public class DisplayConditionScriptView : ReactiveUserControl<DisplayConditionScriptViewModel>
{
    public DisplayConditionScriptView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}