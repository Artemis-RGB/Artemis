using System;
using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Profile;

public partial class ProfileListView : ReactiveUserControl<ProfileListViewModel>
{
    public ProfileListView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen ?? ViewModel?.EntryListViewModel))
            .DisposeWith(d));
    }
}