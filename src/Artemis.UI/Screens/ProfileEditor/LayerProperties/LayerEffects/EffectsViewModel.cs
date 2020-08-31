using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.LayerEffects
{
    public class EffectsViewModel : PropertyChangedBase
    {
        private readonly IPluginService _pluginService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IRenderElementService _renderElementService;
        private BindableCollection<LayerEffectDescriptor> _layerEffectDescriptors;
        private LayerEffectDescriptor _selectedLayerEffectDescriptor;

        public EffectsViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IPluginService pluginService, IRenderElementService renderElementService, IProfileEditorService profileEditorService)
        {
            _pluginService = pluginService;
            _renderElementService = renderElementService;
            _profileEditorService = profileEditorService;
            LayerPropertiesViewModel = layerPropertiesViewModel;
            LayerEffectDescriptors = new BindableCollection<LayerEffectDescriptor>();
            PropertyChanged += HandleSelectedLayerEffectChanged;
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel { get; }
        public bool HasLayerEffectDescriptors => LayerEffectDescriptors.Any();

        public BindableCollection<LayerEffectDescriptor> LayerEffectDescriptors
        {
            get => _layerEffectDescriptors;
            set => SetAndNotify(ref _layerEffectDescriptors, value);
        }

        public LayerEffectDescriptor SelectedLayerEffectDescriptor
        {
            get => _selectedLayerEffectDescriptor;
            set => SetAndNotify(ref _selectedLayerEffectDescriptor, value);
        }

        public void PopulateDescriptors()
        {
            var layerBrushProviders = _pluginService.GetPluginsOfType<LayerEffectProvider>();
            var descriptors = layerBrushProviders.SelectMany(l => l.LayerEffectDescriptors).ToList();
            LayerEffectDescriptors.AddRange(descriptors.Except(LayerEffectDescriptors));
            LayerEffectDescriptors.RemoveRange(LayerEffectDescriptors.Except(descriptors));

            // Sort by display name
            var index = 0;
            foreach (var layerEffectDescriptor in LayerEffectDescriptors.OrderBy(d => d.DisplayName).ToList())
            {
                if (LayerEffectDescriptors.IndexOf(layerEffectDescriptor) != index)
                    LayerEffectDescriptors.Move(LayerEffectDescriptors.IndexOf(layerEffectDescriptor), index);
                index++;
            }

            SelectedLayerEffectDescriptor = null;
            NotifyOfPropertyChange(nameof(HasLayerEffectDescriptors));
        }

        private void HandleSelectedLayerEffectChanged(object sender, PropertyChangedEventArgs e)
        {
            RenderProfileElement renderElement;
            if (LayerPropertiesViewModel.SelectedLayer != null)
                renderElement = LayerPropertiesViewModel.SelectedLayer;
            else if (LayerPropertiesViewModel.SelectedFolder != null)
                renderElement = LayerPropertiesViewModel.SelectedFolder;
            else
                return;

            if (e.PropertyName == nameof(SelectedLayerEffectDescriptor) && SelectedLayerEffectDescriptor != null)
            {
                // Let the fancy animation run
                Execute.PostToUIThread(async () =>
                {
                    await Task.Delay(500);
                    _renderElementService.AddLayerEffect(renderElement, SelectedLayerEffectDescriptor);
                    _profileEditorService.UpdateSelectedProfileElement();
                });
            }
        }
    }
}