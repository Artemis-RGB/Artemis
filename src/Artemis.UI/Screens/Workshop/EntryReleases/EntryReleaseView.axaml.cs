using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.EntryReleases;

public partial class EntryReleaseView : ReactiveUserControl<EntryReleaseViewModel>
{
    public EntryReleaseView()
    {
        InitializeComponent();
    }
}