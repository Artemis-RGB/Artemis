using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Events;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using Artemis.UI.Shared.Utilities;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class BrushPropertyInputViewModel : PropertyInputViewModel<LayerBrushReference>
    {
        private readonly ILayerService _layerService;
        private readonly IPluginService _pluginService;

        public BrushPropertyInputViewModel(LayerPropertyViewModel<LayerBrushReference> layerPropertyViewModel, ILayerService layerService, IPluginService pluginService)
            : base(layerPropertyViewModel)
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

        private void PluginServiceOnPluginLoaded(object sender, PluginEventArgs e)
        {
            UpdateEnumValues();
        }
    }
}