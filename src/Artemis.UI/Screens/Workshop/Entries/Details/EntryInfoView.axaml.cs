using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntryInfoView : ReactiveUserControl<EntryInfoViewModel>
{
    public EntryInfoView()
    {
        InitializeComponent();
    }
}