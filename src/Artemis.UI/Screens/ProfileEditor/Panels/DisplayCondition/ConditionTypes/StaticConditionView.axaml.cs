using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;

public class StaticConditionView : ReactiveUserControl<StaticConditionViewModel>
{
    public StaticConditionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}