using System.Collections.Generic;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.UI.Shared.Utilities;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrushViewModel : LayerBrushViewModel
    {
        public NoiseBrushViewModel(NoiseBrush brush) : base(brush)
        {
            Brush = brush;
        }

        public new NoiseBrush Brush { get; }
        public IEnumerable<ValueDescription> BlendModes => EnumUtilities.GetAllValuesAndDescriptions(typeof(SKBlendMode));
    }
}