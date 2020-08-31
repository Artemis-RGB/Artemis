﻿using Artemis.Core;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class ColorMatrixEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription]
        public LayerProperty<float[]> ColorMatrix { get; set; }

        protected override void PopulateDefaults()
        {
            // Set a gray scale value as default
            ColorMatrix.DefaultValue = new[]
            {
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0.21f, 0.72f, 0.07f, 0, 0,
                0, 0, 0, 1, 0
            };
        }

        protected override void EnableProperties()
        {
            ColorMatrix.IsHidden = true;
        }

        protected override void DisableProperties()
        {
        }
    }
}