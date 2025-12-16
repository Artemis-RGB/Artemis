using Avalonia.Input;
using ReactiveUI.Avalonia;

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