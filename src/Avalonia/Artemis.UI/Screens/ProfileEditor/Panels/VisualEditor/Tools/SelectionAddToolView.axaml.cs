using Artemis.UI.Shared.Events;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Skia;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public class SelectionAddToolView : ReactiveUserControl<SelectionAddToolViewModel>
{
    public SelectionAddToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        ViewModel?.AddLedsInRectangle(e.Rectangle.ToSKRect());
    }
}