using Artemis.UI.Shared.Events;
using Avalonia.ReactiveUI;
using Avalonia.Skia;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.VisualEditor.Tools;

public partial class SelectionRemoveToolView : ReactiveUserControl<SelectionRemoveToolViewModel>
{
    public SelectionRemoveToolView()
    {
        InitializeComponent();
    }


    private void SelectionRectangle_OnSelectionFinished(object? sender, SelectionRectangleEventArgs e)
    {
        ViewModel?.RemoveLedsInRectangle(e.Rectangle.ToSKRect());
    }
}