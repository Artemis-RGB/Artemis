using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;

namespace Artemis.UI.Screens.ProfileEditor.ConfigurationPanels.Section;

public partial class ConfigurationStringItemView : ReactiveUserControl<ConfigurationStringItemViewModel>
{
    public ConfigurationStringItemView()
    {
        InitializeComponent();
    }
}