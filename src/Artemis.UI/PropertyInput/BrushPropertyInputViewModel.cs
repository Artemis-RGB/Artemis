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
        private readonly IRenderElementService _renderElementService;
        private readonly IPluginService _pluginService;
        private List<LayerBrushDescriptor> _descriptors;

        public BrushPropertyInputViewModel(LayerProperty<LayerBrushReference> layerProperty, IProfileEditorService profileEditorService,
            IRenderElementService renderElementService, IPluginService pluginService) : base(layerProperty, profileEditorService)
        {
            _renderElementService = renderElementService;
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
            {
                _renderElementService.RemoveLayerBrush(layer);
                _renderElementService.InstantiateLayerBrush(layer);
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