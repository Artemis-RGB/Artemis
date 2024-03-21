using System;
using System.Reactive.Disposables;
using System.Threading;
using Artemis.UI.Shared.Routing;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Tabs;

public partial class ProfileListView : ReactiveUserControl<ProfileListViewModel>
{
    public ProfileListView()
    {
        InitializeComponent();
        EntriesScrollViewer.SizeChanged += (_, _) => UpdateEntriesPerFetch();

        this.WhenActivated(d =>
        {
            UpdateEntriesPerFetch();
            ViewModel.WhenAnyValue(vm => vm.Screen).WhereNotNull().Subscribe(Navigate).DisposeWith(d);
        });
    }

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        // When near the bottom of EntriesScrollViewer, call FetchMore on the view model
        if (EntriesScrollViewer.Offset.Y != 0 && EntriesScrollViewer.Extent.Height - (EntriesScrollViewer.Viewport.Height + EntriesScrollViewer.Offset.Y) < 100)
            ViewModel?.FetchMore(CancellationToken.None);
    }

    private void Navigate(RoutableScreen viewModel)
    {
        Dispatcher.UIThread.Invoke(() => RouterFrame.NavigateFromObject(viewModel), DispatcherPriority.ApplicationIdle);
    }

    private void UpdateEntriesPerFetch()
    {
        if (ViewModel != null)
            ViewModel.EntriesPerFetch = (int) (EntriesScrollViewer.Viewport.Height / 120);
    }
}