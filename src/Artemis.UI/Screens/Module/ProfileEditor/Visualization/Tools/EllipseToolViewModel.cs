using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.UI.Properties;
using Artemis.UI.Services.Interfaces;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization.Tools
{
    public class EllipseToolViewModel : VisualizationToolViewModel
    {
        private bool _shiftDown;

        public EllipseToolViewModel(ProfileViewModel profileViewModel, IProfileEditorService profileEditorService) : base(profileViewModel, profileEditorService)
        {
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
                GetShapePosition(out var point, out var size);
                layer.LayerShape = new Ellipse(layer) {Size = size, Position = point};
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

        private void GetShapePosition(out SKPoint point, out SKSize size)
        {
            var layer = (Layer) ProfileEditorService.SelectedProfileElement;
            var x = layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            var widthScale = width / 100f;
            var heightScale = height / 100f;
            var rect = new Rect(
                x - width / DragRectangle.X,
                y - height / DragRectangle.Y,
                width / DragRectangle.Width,
                height / DragRectangle.Height
            );
            point = new SKPoint(0.5f,0.5f);
            size = new SKSize((float) (DragRectangle.Width * widthScale), (float) (DragRectangle.Height * heightScale));
        }
    }
}