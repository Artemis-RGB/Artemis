using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.StatusBar;

public partial class StatusBarView : ReactiveUserControl<StatusBarViewModel>
{
    public StatusBarView()
    {
        InitializeComponent();
    }

}