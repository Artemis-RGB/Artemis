using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using SkiaSharp;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput
{
    public class SKSizePropertyInputViewModel : PropertyInputViewModel
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKSize)};

        protected override void UpdateBaseValue(object value)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateKeyframeValue(BaseKeyframe keyframe, object value)
        {
            throw new NotImplementedException();
        }

        protected override void CreateKeyframeForValue(object value)
        {
            throw new NotImplementedException();
        }
    }
}