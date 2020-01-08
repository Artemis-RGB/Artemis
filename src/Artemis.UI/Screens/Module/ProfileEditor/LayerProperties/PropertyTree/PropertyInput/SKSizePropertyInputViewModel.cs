using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class SKSizePropertyInputViewModel : PropertyInputViewModel
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKSize)};
    }
}