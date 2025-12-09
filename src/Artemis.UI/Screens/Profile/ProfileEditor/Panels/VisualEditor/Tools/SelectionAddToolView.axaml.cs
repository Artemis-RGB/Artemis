using Artemis.UI.Shared.Events;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Avalonia.Skia;

namespace Artemis.UI.Screens.Profile.ProfileEditor.VisualEditor.Tools;

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