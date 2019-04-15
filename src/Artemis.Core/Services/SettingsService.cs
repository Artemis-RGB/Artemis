using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Repositories;

namespace Artemis.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private SettingRepository _settingRepository;

        public SettingsService()
        {
            _settingRepository = new SettingRepository();
        }

        public PluginSettings GetPluginSettings(PluginInfo pluginInfo)
        {
            return new PluginSettings(pluginInfo, _settingRepository);
        }
    }

    public interface ISettingsService : IArtemisService
    {
    }
}