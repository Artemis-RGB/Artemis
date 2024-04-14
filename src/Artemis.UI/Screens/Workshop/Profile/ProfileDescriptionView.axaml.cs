using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Profile;

public partial class ProfileDescriptionView : ReactiveUserControl<ProfileDescriptionViewModel>
{
    public ProfileDescriptionView()
    {
        InitializeComponent();
    }
}