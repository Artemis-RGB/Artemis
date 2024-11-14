using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.List;

public partial class EntryListItemVerticalView : ReactiveUserControl<EntryListItemVerticalViewModel>
{
    public EntryListItemVerticalView()
    {
        InitializeComponent();
    }
}