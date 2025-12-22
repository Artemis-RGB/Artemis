using Artemis.UI.Shared.Events;
using Avalonia.Input;
using ReactiveUI.Avalonia;
using Avalonia.Skia;

namespace Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;

public partial class SelectionAddToolView : ReactiveUserControl<SelectionAddToolViewModel>
{
    public SelectionAddToolView()
    {
        InitializeComponent();
    }


    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        ViewModel?.AddLedsInRectangle(e.Rectangle.ToSKRect(), e.KeyModifiers.HasFlag(KeyModifiers.Shift), e.KeyModifiers.HasFlag(KeyModifiers.Control));
    }
}