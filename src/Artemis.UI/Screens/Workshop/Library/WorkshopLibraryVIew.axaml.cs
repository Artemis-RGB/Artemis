using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class WorkshopLibraryView : ReactiveUserControl<WorkshopLibraryViewModel>
{
    private int _lastIndex;

    public WorkshopLibraryView()
    {
        InitializeComponent();
        this.WhenActivated(d => { ViewModel.WhenAnyValue(vm => vm.Screen).WhereNotNull().Subscribe(Navigate).DisposeWith(d); });
    }

    private void Navigate(ViewModelBase viewModel)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (ViewModel == null)
                return;
            
            SlideNavigationTransitionInfo transitionInfo = new()
            {
                Effect = ViewModel.Tabs.IndexOf(ViewModel.SelectedTab) > _lastIndex ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft
            };
            TabFrame.NavigateFromObject(viewModel, new FrameNavigationOptions {TransitionInfoOverride = transitionInfo});
            _lastIndex = ViewModel.Tabs.IndexOf(ViewModel.SelectedTab);
        });
    }
}