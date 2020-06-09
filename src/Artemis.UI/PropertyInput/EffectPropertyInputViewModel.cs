using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerEffect;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.PropertyInput
{
    public class EffectPropertyInputViewModel : PropertyInputViewModel<LayerEffectReference>
    {
        private readonly ILayerService _layerService;
        private readonly IPluginService _pluginService;

        public EffectPropertyInputViewModel(LayerProperty<LayerEffectReference> layerProperty, IProfileEditorService profileEditorService,
            ILayerService layerService, IPluginService pluginService) : base(layerProperty, profileEditorService)
        {
            _layerService = layerService;
            _pluginService = pluginService;

            _pluginService.PluginEnabled += PluginServiceOnPluginLoaded;
            _pluginService.PluginDisabled += PluginServiceOnPluginLoaded;
            UpdateEnumValues();
        }

        public List<LayerEffectDescriptor> Descriptors { get; set; }

        public LayerEffectDescriptor SelectedDescriptor
        {
            get => Descriptors.FirstOrDefault(d => d.LayerEffectProvider.PluginInfo.Guid == InputValue?.EffectPluginGuid && d.LayerEffectType.Name == InputValue?.EffectType);
            set => SetEffectByDescriptor(value);
        }

        public void UpdateEnumValues()
        {
            var layerEffectProviders = _pluginService.GetPluginsOfType<LayerEffectProvider>();
            Descriptors = layerEffectProviders.SelectMany(l => l.LayerEffectDescriptors).ToList();
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
            _layerService.InstantiateLayerEffect(LayerProperty.Layer);
        }

        private void SetEffectByDescriptor(LayerEffectDescriptor value)
        {
            InputValue = new LayerEffectReference {EffectPluginGuid = value.LayerEffectProvider.PluginInfo.Guid, EffectType = value.LayerEffectType.Name};
        }

        private void PluginServiceOnPluginLoaded(object sender, PluginEventArgs e)
        {
            UpdateEnumValues();
        }
    }
}