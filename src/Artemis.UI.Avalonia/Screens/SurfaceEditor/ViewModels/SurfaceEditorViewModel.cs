using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels
{
    public class SurfaceEditorViewModel : MainScreenViewModel
    {
        public SurfaceEditorViewModel(IScreen hostScreen) : base(hostScreen, "surface-editor")
        {
            DisplayName = "Surface Editor";
        }
    }
}