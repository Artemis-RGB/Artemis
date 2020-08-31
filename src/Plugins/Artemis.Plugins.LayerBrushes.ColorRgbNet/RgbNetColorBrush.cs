using Artemis.Core;
using Artemis.Core.LayerBrushes;
using RGB.NET.Brushes;
using RGB.NET.Core;

namespace Artemis.Plugins.LayerBrushes.ColorRgbNet
{
    public class RgbNetColorBrush : RgbNetLayerBrush<RgbNetColorBrushProperties>
    {
        private SolidColorBrush _solidBrush;

        public override void EnableLayerBrush()
        {
            _solidBrush = new SolidColorBrush(Color.Transparent);
        }

        public override void DisableLayerBrush()
        {
            _solidBrush = null;
        }

        public override void Update(double deltaTime)
        {
            _solidBrush.Color = Properties.Color.CurrentValue.ToRgbColor();
        }

        public override IBrush GetBrush()
        {
            return _solidBrush;
        }
    }
}