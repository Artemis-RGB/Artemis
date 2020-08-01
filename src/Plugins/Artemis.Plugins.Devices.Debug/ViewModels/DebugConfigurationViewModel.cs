using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;
using Artemis.Plugins.Devices.Debug.Settings;
using Stylet;

namespace Artemis.Plugins.Devices.Debug.ViewModels
{
    public class DebugConfigurationViewModel : PluginConfigurationViewModel
    {
        private PluginSetting<List<DeviceDefinition>> _definitions;

        public DebugConfigurationViewModel(Plugin plugin, PluginSettings settings) : base(plugin)
        {
            _definitions = settings.GetSetting<List<DeviceDefinition>>("DeviceDefinitions");
            Definitions = new BindableCollection<DeviceDefinition>(_definitions.Value);
        }

        public BindableCollection<DeviceDefinition> Definitions { get; }

        public void SaveChanges()
        {
            // Ignore empty definitions
            _definitions.Value.Clear();
            _definitions.Value.AddRange(Definitions.Where(d => !string.IsNullOrWhiteSpace(d.Layout) || !string.IsNullOrWhiteSpace(d.ImageLayout)));
            _definitions.Save();

            RequestClose();
        }

        public void Cancel()
        {
            _definitions.RejectChanges();
            RequestClose();
        }
    }
}