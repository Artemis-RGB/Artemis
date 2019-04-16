using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Repositories;

namespace Artemis.Core.Services
{
    // TODO: Rethink this :')
    public class SettingsService : ISettingsService
    {
        private SettingRepository _settingRepository;

        public SettingsService()
        {
            _settingRepository = new SettingRepository();
        }

        public PluginSettingsContainer GetPluginSettings(PluginInfo pluginInfo)
        {
            return new PluginSettingsContainer(pluginInfo, _settingRepository);
        }
    }

    public interface ISettingsService : IArtemisService
    {
    }
}