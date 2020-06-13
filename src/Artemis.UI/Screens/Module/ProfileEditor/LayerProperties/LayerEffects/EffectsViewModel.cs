using System.Linq;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerEffect;
using Artemis.Core.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.LayerEffects
{
    public class EffectsViewModel : PropertyChangedBase
    {
        private readonly ILayerService _layerService;
        private readonly IPluginService _pluginService;

        public EffectsViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IPluginService pluginService, ILayerService layerService)
        {
            _pluginService = pluginService;
            _layerService = layerService;
            LayerPropertiesViewModel = layerPropertiesViewModel;
            LayerEffectDescriptors = new BindableCollection<LayerEffectDescriptor>();
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }

        public BindableCollection<LayerEffectDescriptor> LayerEffectDescriptors { get; set; }
        public bool HasLayerEffectDescriptors => LayerEffectDescriptors.Any();

        public LayerEffectDescriptor SelectedLayerEffectDescriptor
        {
            get => null;
            set => AddLayerEffect(value);
        }

        public void PopulateDescriptors()
        {
            var layerBrushProviders = _pluginService.GetPluginsOfType<LayerEffectProvider>();

            if (LayerEffectDescriptors.Any())
                LayerEffectDescriptors.Clear();
            LayerEffectDescriptors.AddRange(layerBrushProviders.SelectMany(l => l.LayerEffectDescriptors));

            NotifyOfPropertyChange(nameof(HasLayerEffectDescriptors));
        }

        private void AddLayerEffect(LayerEffectDescriptor value)
        {
            if (LayerPropertiesViewModel.SelectedLayer != null && value != null)
                _layerService.AddLayerEffect(LayerPropertiesViewModel.SelectedLayer, value);
        }
    }
}