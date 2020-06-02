using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.Models;
using Artemis.Plugins.LayerBrushes.ColorRgbNet.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.Plugins.LayerBrushes.ColorRgbNet
{
    public class RgbNetColorBrushProvider : LayerBrushProvider
    {
        private readonly IProfileEditorService _profileEditorService;

        public RgbNetColorBrushProvider(PluginInfo pluginInfo, IProfileEditorService profileEditorService) : base(pluginInfo)
        {
            _profileEditorService = profileEditorService;
            AddLayerBrushDescriptor<RgbNetColorBrush>("RGB.NET Color", "A RGB.NET based color", "Brush");
        }

        protected override void EnablePlugin()
        {
            _profileEditorService.RegisterPropertyInput(PluginInfo, typeof(StringPropertyInputViewModel));
        }

        protected override void DisablePlugin()
        {
        }
    }
}