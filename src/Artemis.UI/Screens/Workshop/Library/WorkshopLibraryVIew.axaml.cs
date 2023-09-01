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
        Dispatcher.UIThread.Invoke(() => TabFrame.NavigateFromObject(viewModel, new FrameNavigationOptions {TransitionInfoOverride = GetTransitionInfo()}));
    }

    private SlideNavigationTransitionInfo GetTransitionInfo()
    {
        if (ViewModel?.SelectedTab == null)
            return new SlideNavigationTransitionInfo();

        SlideNavigationTransitionEffect effect = ViewModel.Tabs.IndexOf(ViewModel.SelectedTab) > _lastIndex ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
        SlideNavigationTransitionInfo info = new() {Effect = effect};
        _lastIndex = ViewModel.Tabs.IndexOf(ViewModel.SelectedTab);
        
        return info;
    }
}