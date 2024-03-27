using System;
using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Profile;

public partial class ProfileDetailsView : ReactiveUserControl<ProfileDetailsViewModel>
{
    public ProfileDetailsView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen ?? ViewModel?.ProfileDescriptionViewModel))
            .DisposeWith(d));
    }
}