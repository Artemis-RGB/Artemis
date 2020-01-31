using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.UI.Properties;
using Artemis.UI.Services.Interfaces;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class FillToolViewModel : VisualizationToolViewModel
    {
        private readonly ILayerEditorService _layerEditorService;

        public FillToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService) : base(profileViewModel, profileEditorService)
        {
            _layerEditorService = layerEditorService;
            using (var stream = new MemoryStream(Resources.aero_fill))
            {
                Cursor = new Cursor(stream);
            }
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);

            // Find out if the click is inside a layer and if so, fill it
            var position = e.GetPosition((IInputElement) sender);
            var panZoomVm = ProfileViewModel.PanZoomViewModel;
            var layer = ProfileEditorService.SelectedProfile
                .GetAllLayers()
                .FirstOrDefault(l => l.Leds.Any(
                    led => panZoomVm.TransformContainingRect(led.RgbLed.AbsoluteLedRectangle).Contains(position))
                );

            if (layer == null)
                return;
            layer.LayerShape = new Fill(layer);

            // Apply the full layer rectangle
            _layerEditorService.SetShapeBaseFromRectangle(layer.LayerShape, _layerEditorService.GetLayerBounds(layer));
            ProfileEditorService.UpdateSelectedProfileElement();
        }
    }
}