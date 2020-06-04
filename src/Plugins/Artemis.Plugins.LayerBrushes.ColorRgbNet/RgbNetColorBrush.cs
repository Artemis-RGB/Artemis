using System;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.LayerBrush.Abstract;
using RGB.NET.Brushes;
using RGB.NET.Core;

namespace Artemis.Plugins.LayerBrushes.ColorRgbNet
{
    public class RgbNetColorBrush : RgbNetLayerBrush<RgbNetColorBrushProperties>
    {
        private readonly SolidColorBrush _solidBrush;

        public RgbNetColorBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            _solidBrush = new SolidColorBrush(Color.Transparent);
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