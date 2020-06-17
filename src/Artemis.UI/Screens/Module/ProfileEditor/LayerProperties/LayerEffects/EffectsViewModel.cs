using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
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
            PropertyChanged += HandleSelectedLayerEffectChanged;
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }

        public BindableCollection<LayerEffectDescriptor> LayerEffectDescriptors { get; set; }
        public bool HasLayerEffectDescriptors => LayerEffectDescriptors.Any();

        public LayerEffectDescriptor SelectedLayerEffectDescriptor { get; set; }

        public void PopulateDescriptors()
        {
            var layerBrushProviders = _pluginService.GetPluginsOfType<LayerEffectProvider>();
            var descriptors = layerBrushProviders.SelectMany(l => l.LayerEffectDescriptors).ToList();
            LayerEffectDescriptors.AddRange(descriptors.Except(LayerEffectDescriptors));
            LayerEffectDescriptors.RemoveRange(LayerEffectDescriptors.Except(descriptors));

            SelectedLayerEffectDescriptor = null;
            NotifyOfPropertyChange(nameof(HasLayerEffectDescriptors));
        }

        private void HandleSelectedLayerEffectChanged(object sender, PropertyChangedEventArgs e)
        {
            EffectProfileElement effectElement;
            if (LayerPropertiesViewModel.SelectedLayer != null)
                effectElement = LayerPropertiesViewModel.SelectedLayer;
            else if (LayerPropertiesViewModel.SelectedFolder != null)
                effectElement = LayerPropertiesViewModel.SelectedFolder;
            else
                return;

            if (e.PropertyName == nameof(SelectedLayerEffectDescriptor) && SelectedLayerEffectDescriptor != null)
            {
                // Let the fancy animation run
                Execute.PostToUIThread(async () =>
                {
                    await Task.Delay(500);
                    _layerService.AddLayerEffect(effectElement, SelectedLayerEffectDescriptor);
                });
            }
        }
    }
}