using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries;

public partial class EntryListView : ReactiveUserControl<EntryListViewModel>
{
    public EntryListView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        await ViewModel.NavigateToEntry();
    }
}