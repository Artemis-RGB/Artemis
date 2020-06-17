using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.PropertyInput
{
    public class BrushPropertyInputViewModel : PropertyInputViewModel<LayerBrushReference>
    {
        private readonly ILayerService _layerService;
        private readonly IPluginService _pluginService;

        public BrushPropertyInputViewModel(LayerProperty<LayerBrushReference> layerProperty, IProfileEditorService profileEditorService,
            ILayerService layerService, IPluginService pluginService) : base(layerProperty, profileEditorService)
        {
            _layerService = layerService;
            _pluginService = pluginService;

            _pluginService.PluginEnabled += PluginServiceOnPluginLoaded;
            _pluginService.PluginDisabled += PluginServiceOnPluginLoaded;
            UpdateEnumValues();
        }

        public List<LayerBrushDescriptor> Descriptors { get; set; }

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
            {
                _layerService.RemoveLayerBrush(layer);
                _layerService.InstantiateLayerBrush(layer);
            }
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