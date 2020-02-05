using System.Collections.Generic;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.UI.Shared.Utilities;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrushViewModel : LayerBrushViewModel
    {
        public ColorBrushViewModel(ColorBrush element) : base(element)
        {
            ColorBrush = element;
        }

        public ColorBrush ColorBrush { get; }
        public IEnumerable<ValueDescription> BrushTypes => EnumUtilities.GetAllValuesAndDescriptions(typeof(GradientType));
    }
}