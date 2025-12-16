using System;
using System.Reactive.Disposables.Fluent;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor;

public partial class ProfileView : ReactiveUserControl<ProfileViewModel>
{
    public ProfileView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .WhereNotNull()
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen))
            .DisposeWith(d));
    }
}