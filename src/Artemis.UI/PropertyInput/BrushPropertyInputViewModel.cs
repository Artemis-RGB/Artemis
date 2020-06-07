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
using Artemis.UI.Shared.Utilities;
using Stylet;

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
            ComboboxValues = new BindableCollection<ValueDescription>();

            _pluginService.PluginLoaded += PluginServiceOnPluginLoaded;
            UpdateEnumValues();
        }

        public BindableCollection<ValueDescription> ComboboxValues { get; }

        public void UpdateEnumValues()
        {
            var layerBrushProviders = _pluginService.GetPluginsOfType<LayerBrushProvider>();
            var descriptors = layerBrushProviders.SelectMany(l => l.LayerBrushDescriptors).ToList();

            var enumValues = new List<ValueDescription>();
            foreach (var layerBrushDescriptor in descriptors)
            {
                var brushName = layerBrushDescriptor.LayerBrushType.Name;
                var brushGuid = layerBrushDescriptor.LayerBrushProvider.PluginInfo.Guid;
                if (InputValue != null && InputValue.BrushType == brushName && InputValue.BrushPluginGuid == brushGuid)
                    enumValues.Add(new ValueDescription {Description = layerBrushDescriptor.DisplayName, Value = InputValue});
                else
                    enumValues.Add(new ValueDescription {Description = layerBrushDescriptor.DisplayName, Value = new LayerBrushReference {BrushType = brushName, BrushPluginGuid = brushGuid}});
            }

            ComboboxValues.Clear();
            ComboboxValues.AddRange(enumValues);
        }

        public override void Dispose()
        {
            _pluginService.PluginLoaded -= PluginServiceOnPluginLoaded;
            base.Dispose();
        }

        protected override void OnInputValueApplied()
        {
            _layerService.InstantiateLayerBrush(LayerProperty.Layer);
        }

        private void PluginServiceOnPluginLoaded(object sender, PluginEventArgs e)
        {
            UpdateEnumValues();
        }
    }
}