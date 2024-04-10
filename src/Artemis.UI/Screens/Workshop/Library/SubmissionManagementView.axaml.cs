using System;
using System.Reactive.Disposables;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionManagementView : ReactiveUserControl<SubmissionManagementViewModel>
{
    public SubmissionManagementView()
    {
        InitializeComponent();
        this.WhenActivated(d => ViewModel.WhenAnyValue(vm => vm.Screen)
            .Subscribe(screen => RouterFrame.NavigateFromObject(screen ?? ViewModel?.DetailsViewModel))
            .DisposeWith(d));
    }
}