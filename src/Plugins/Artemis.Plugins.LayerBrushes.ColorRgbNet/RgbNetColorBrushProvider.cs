using Artemis.Core.Plugins.LayerBrushes;
using Artemis.Plugins.LayerBrushes.ColorRgbNet.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.Plugins.LayerBrushes.ColorRgbNet
{
    public class RgbNetColorBrushProvider : LayerBrushProvider
    {
        private readonly IProfileEditorService _profileEditorService;

        public RgbNetColorBrushProvider(IProfileEditorService profileEditorService)
        {
            _profileEditorService = profileEditorService;
        }

        public override void EnablePlugin()
        {
            _profileEditorService.RegisterPropertyInput<StringPropertyInputViewModel>(PluginInfo);
            RegisterLayerBrushDescriptor<RgbNetColorBrush>("RGB.NET Color", "A RGB.NET based color", "Brush");
        }

        public override void DisablePlugin()
        {
        }
    }
}