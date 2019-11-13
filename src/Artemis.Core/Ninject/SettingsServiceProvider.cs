using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services;
using Ninject;
using Ninject.Activation;

namespace Artemis.Core.Ninject
{
    public class SettingsServiceProvider : Provider<ISettingsService>
    {
        private readonly SettingsService _instance;

        public SettingsServiceProvider(IKernel kernel)
        {
            // This is not lazy, but the core is always going to be using this anyway
            _instance = kernel.Get<SettingsService>();
        }

        protected override ISettingsService CreateInstance(IContext context)
        {
            var parentRequest = context.Request.ParentRequest;
            if (parentRequest == null || typeof(Plugin).IsAssignableFrom(parentRequest.Service))
                throw new ArtemisPluginException($"SettingsService can not be injected into a plugin. Inject {nameof(PluginSettings)} instead.");

            return _instance;
        }
    }
}