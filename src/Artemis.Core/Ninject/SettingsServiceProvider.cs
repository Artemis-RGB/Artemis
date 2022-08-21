using Artemis.Core.Services;
using Ninject;
using Ninject.Activation;

namespace Artemis.Core.Ninject;

internal class SettingsServiceProvider : Provider<ISettingsService>
{
    private readonly SettingsService _instance;

    public SettingsServiceProvider(IKernel kernel)
    {
        // This is not lazy, but the core is always going to be using this anyway
        _instance = kernel.Get<SettingsService>();
    }

    protected override ISettingsService CreateInstance(IContext context)
    {
        IRequest parentRequest = context.Request.ParentRequest;
        if (parentRequest == null || typeof(PluginFeature).IsAssignableFrom(parentRequest.Service))
            throw new ArtemisPluginException($"SettingsService can not be injected into a plugin. Inject {nameof(PluginSettings)} instead.");

        return _instance;
    }
}