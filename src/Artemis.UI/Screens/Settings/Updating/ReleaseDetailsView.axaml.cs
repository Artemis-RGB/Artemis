using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings.Updating;

public partial class ReleaseDetailsView : ReactiveUserControl<ReleaseDetailsViewModel>
{
    public ReleaseDetailsView()
    {
        InitializeComponent();
    }

    
    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ViewModel?.NavigateToSource();
    }
}