using Artemis.UI.Shared.Events;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Skia;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public partial class SelectionRemoveToolView : ReactiveUserControl<SelectionRemoveToolViewModel>
{
    public SelectionRemoveToolView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        ViewModel?.RemoveLedsInRectangle(e.Rectangle.ToSKRect());
    }
}