using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.StatusBar;

public class StatusBarView : ReactiveUserControl<StatusBarViewModel>
{
    public StatusBarView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}