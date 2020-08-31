using System.Diagnostics;
using Artemis.Core.LayerEffects;

namespace Artemis.Plugins.LayerEffects.Filter.ViewModels
{
    public class ColorMatrixConfigurationViewModel : EffectConfigurationViewModel
    {
        public ColorMatrixConfigurationViewModel(ColorMatrixEffect layerEffect) : base(layerEffect)
        {
            Properties = layerEffect.Properties;
        }

        public ColorMatrixEffectProperties Properties { get; set; }

        public void OpenGuide()
        {
            Process.Start(new ProcessStartInfo("cmd", "/c start https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/color-filters")
            {
                CreateNoWindow = true
            });
        }
    }
}