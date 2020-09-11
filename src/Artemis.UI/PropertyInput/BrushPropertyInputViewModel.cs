using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.PropertyInput
{
    public class BrushPropertyInputViewModel : PropertyInputViewModel<LayerBrushReference>
    {
        private readonly IPluginService _pluginService;
        private List<LayerBrushDescriptor> _descriptors;

        public BrushPropertyInputViewModel(LayerProperty<LayerBrushReference> layerProperty,
            IProfileEditorService profileEditorService, 
            IPluginService pluginService) : base(layerProperty, profileEditorService)
        {
            _pluginService = pluginService;

            _pluginService.PluginEnabled += PluginServiceOnPluginLoaded;
            _pluginService.PluginDisabled += PluginServiceOnPluginLoaded;
            UpdateEnumValues();
        }

        public List<LayerBrushDescriptor> Descriptors
        {
            get => _descriptors;
            set => SetAndNotify(ref _descriptors, value);
        }

        public LayerBrushDescriptor SelectedDescriptor
        {
            get => Descriptors.FirstOrDefault(d => d.LayerBrushProvider.PluginInfo.Guid == InputValue?.BrushPluginGuid && d.LayerBrushType.Name == InputValue?.BrushType);
            set => SetBrushByDescriptor(value);
        }

        public void UpdateEnumValues()
        {
            var layerBrushProviders = _pluginService.GetPluginsOfType<LayerBrushProvider>();
            Descriptors = layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors).ToList();
            NotifyOfPropertyChange(nameof(SelectedDescriptor));
        }


        public override void Dispose()
        {
            _pluginService.PluginEnabled -= PluginServiceOnPluginLoaded;
            _pluginService.PluginDisabled -= PluginServiceOnPluginLoaded;
            base.Dispose();
        }

        protected override void OnInputValueApplied()
        {
            if (LayerProperty.ProfileElement is Layer layer) 
                layer.ChangeLayerBrush(SelectedDescriptor);
        }

        private void SetBrushByDescriptor(LayerBrushDescriptor value)
        {
            InputValue = new LayerBrushReference {BrushPluginGuid = value.LayerBrushProvider.PluginInfo.Guid, BrushType = value.LayerBrushType.Name};
        }

        private void PluginServiceOnPluginLoaded(object sender, PluginEventArgs e)
        {
            UpdateEnumValues();
        }
    }
}