using Avalonia;
using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntryImagesView : ReactiveUserControl<EntryImagesViewModel>
{
    public EntryImagesView()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not IDataContextProvider contextProvider)
            return;
        if (contextProvider.DataContext is not EntryImageViewModel entryImageViewModel)
            return;

        ViewModel?.ShowImages(entryImageViewModel);
    }
}