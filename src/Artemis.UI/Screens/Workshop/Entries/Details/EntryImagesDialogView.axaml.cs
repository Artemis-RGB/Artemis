using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntryImagesDialogView : ReactiveUserControl<EntryImagesDialogViewModel>
{
    public EntryImagesDialogView()
    {
        InitializeComponent();
    }
}