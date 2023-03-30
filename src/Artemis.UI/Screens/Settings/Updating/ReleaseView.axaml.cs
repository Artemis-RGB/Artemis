using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings.Updating;

public partial class ReleaseView : ReactiveUserControl<ReleaseViewModel>
{
    public ReleaseView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ViewModel?.NavigateToSource();
    }
}