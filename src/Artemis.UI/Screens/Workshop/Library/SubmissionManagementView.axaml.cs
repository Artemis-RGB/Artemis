using System;
using System.Reactive.Disposables.Fluent;
using ReactiveUI.Avalonia;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionManagementView : ReactiveUserControl<SubmissionManagementViewModel>
{
    public SubmissionManagementView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .WhereNotNull()
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen))
            .DisposeWith(d));
    }
}