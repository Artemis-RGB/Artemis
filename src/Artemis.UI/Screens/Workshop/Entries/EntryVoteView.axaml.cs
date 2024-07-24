using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries;

public partial class EntryVoteView : ReactiveUserControl<EntryVoteViewModel>
{
    public EntryVoteView()
    {
        InitializeComponent();
    }
}