using Avalonia;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Categories;

public partial class CategoriesView : ReactiveUserControl<CategoriesViewModel>
{
    public CategoriesView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left && sender is IDataContextProvider p && p.DataContext is CategoryViewModel categoryViewModel)
            categoryViewModel.IsSelected = !categoryViewModel.IsSelected;
    }
}