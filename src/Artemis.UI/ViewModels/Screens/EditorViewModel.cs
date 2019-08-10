using Artemis.Core.Services.Interfaces;
using Artemis.UI.ViewModels.Controls.Editor;
using Artemis.UI.ViewModels.Interfaces;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class EditorViewModel : Screen, IEditorViewModel
    {
        public EditorViewModel(IRgbService rgbService)
        {
            SurfaceEditorViewModel = new SurfaceEditorViewModel(rgbService);
        }

        public SurfaceEditorViewModel SurfaceEditorViewModel { get; set; }
        public string Title => "Editor";
    }
}