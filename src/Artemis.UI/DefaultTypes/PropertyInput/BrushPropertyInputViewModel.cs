using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class BrushPropertyInputViewModel : PropertyInputViewModel<LayerBrushReference>
    {
        private readonly IPluginService _pluginService;
        private BindableCollection<LayerBrushDescriptor> _descriptors;

        public BrushPropertyInputViewModel(LayerProperty<LayerBrushReference> layerProperty,
            IProfileEditorService profileEditorService,
            IPluginService pluginService) : base(layerProperty, profileEditorService)
        {
            _pluginService = pluginService;

            _pluginService.PluginEnabled += PluginServiceOnPluginLoaded;
            _pluginService.PluginDisabled += PluginServiceOnPluginLoaded;
            UpdateEnumValues();
        }

        public BindableCollection<LayerBrushDescriptor> Descriptors
        {
            get => _descriptors;
            set => SetAndNotify(ref _descriptors, value);
        }

        public LayerBrushDescriptor SelectedDescriptor
        {
            get => Descriptors.FirstOrDefault(d => d.MatchesLayerBrushReference(InputValue));
            set => SetBrushByDescriptor(value);
        }

        public void UpdateEnumValues()
        {
            List<LayerBrushProvider> layerBrushProviders = _pluginService.GetPluginsOfType<LayerBrushProvider>();
            Descriptors = new BindableCollection<LayerBrushDescriptor>(layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors));
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
            InputValue = new LayerBrushReference(value);
        }

        private void PluginServiceOnPluginLoaded(object sender, PluginEventArgs e)
        {
            UpdateEnumValues();
        }
    }
}