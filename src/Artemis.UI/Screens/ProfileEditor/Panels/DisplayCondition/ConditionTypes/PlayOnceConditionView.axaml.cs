using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;

public class PlayOnceConditionView : UserControl
{
    public PlayOnceConditionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}