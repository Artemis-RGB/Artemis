using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Models;
using Artemis.Storage.Repositories;
using Ninject.Activation;

namespace Artemis.Core.Ninject
{
    public class PluginSettingsProvider : Provider<PluginSettings>
    {
        private readonly ISettingRepository _settingRepository;

        public PluginSettingsProvider(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        protected override PluginSettings CreateInstance(IContext context)
        {
            var pluginInfo = context.Request.ParentRequest?.Parameters.FirstOrDefault(p => p.Name == "PluginInfo")?.GetValue(context, null) as PluginInfo;
            if (pluginInfo == null)
                throw new ArtemisCoreException("A plugin needs to be initialized with PluginInfo as a parameter");

            return new PluginSettings(pluginInfo, _settingRepository);
        }
    }
}