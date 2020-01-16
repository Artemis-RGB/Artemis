using System.IO;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.UI.Properties;
using Artemis.UI.Services;
using Artemis.UI.Services.Interfaces;
using SkiaSharp.Views.WPF;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class RectangleToolViewModel : VisualizationToolViewModel
    {
        private readonly ILayerEditorService _layerEditorService;
        private bool _shiftDown;

        public RectangleToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService) 
            : base(profileViewModel, profileEditorService)
        {
            _layerEditorService = layerEditorService;
            using (var stream = new MemoryStream(Resources.aero_crosshair))
            {
                Cursor = new Cursor(stream);
            }
        }

        public Rect DragRectangle { get; set; }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            if (!IsMouseDown)
                return;

            var position = ProfileViewModel.PanZoomViewModel.GetRelativeMousePosition(sender, e);
            if (!_shiftDown)
                DragRectangle = new Rect(MouseDownStartPosition, position);
            else
                DragRectangle = GetSquareRectBetweenPoints(MouseDownStartPosition, position);
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);

            if (ProfileEditorService.SelectedProfileElement is Layer layer)
            {
                // Ensure the shape is a rectangle
                if (!(layer.LayerShape is Rectangle))
                    layer.LayerShape = new Rectangle(layer);

                // Apply the drag rectangle
                _layerEditorService.SetShapeRenderRect(layer.LayerShape, DragRectangle);
                ProfileEditorService.UpdateSelectedProfileElement();
            }
        }

        public override void KeyUp(KeyEventArgs e)
        {
            base.KeyUp(e);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _shiftDown = false;
        }

        public override void KeyDown(KeyEventArgs e)
        {
            base.KeyDown(e);

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                _shiftDown = true;
        }
    }
}