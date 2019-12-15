using System.IO;
using System.Windows.Input;
using Artemis.UI.Properties;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class PolygonToolViewModel : VisualizationToolViewModel
    {
        public PolygonToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
            using (var stream = new MemoryStream(Resources.aero_crosshair))
            {
                Cursor = new Cursor(stream);
            }
        }
    }
}