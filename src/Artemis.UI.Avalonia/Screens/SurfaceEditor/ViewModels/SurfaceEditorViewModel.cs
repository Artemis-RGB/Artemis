using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.SurfaceEditor.ViewModels
{
    public class SurfaceEditorViewModel : MainScreenViewModel
    {
        public SurfaceEditorViewModel(IScreen hostScreens) : base(hostScreens, "surface-editor")
        {
            DisplayName = "Surface Editor";
        }
    }
}