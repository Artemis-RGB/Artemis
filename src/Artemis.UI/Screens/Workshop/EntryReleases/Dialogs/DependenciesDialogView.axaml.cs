using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.EntryReleases.Dialogs;

public partial class DependenciesDialogView : ReactiveUserControl<DependenciesDialogViewModel>
{
    public DependenciesDialogView()
    {
        InitializeComponent();
    }
}