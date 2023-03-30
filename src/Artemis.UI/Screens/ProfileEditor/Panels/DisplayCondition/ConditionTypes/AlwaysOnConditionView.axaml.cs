using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;

public partial class AlwaysOnConditionView : UserControl
{
    public AlwaysOnConditionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}