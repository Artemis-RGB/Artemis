using System.Threading;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.List;

public partial class EntryListView : ReactiveUserControl<EntryListViewModel>
{
    public EntryListView()
    {
        InitializeComponent();
        EntriesScrollViewer.SizeChanged += (_, _) => UpdateEntriesPerFetch();
        
        this.WhenActivated(_ => UpdateEntriesPerFetch());
    }

    private void ScrollViewer_OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (ViewModel == null)
            return;
        
        // When near the bottom of EntriesScrollViewer, call FetchMore on the view model
        if (EntriesScrollViewer.Offset.Y != 0 && EntriesScrollViewer.Extent.Height - (EntriesScrollViewer.Viewport.Height + EntriesScrollViewer.Offset.Y) < 100)
            ViewModel.FetchMore(CancellationToken.None);

        ViewModel.ScrollOffset = EntriesScrollViewer.Offset;
    }

    private void UpdateEntriesPerFetch()
    {
        if (ViewModel != null)
            ViewModel.EntriesPerFetch = (int) (EntriesScrollViewer.Viewport.Height / 120);
    }
}