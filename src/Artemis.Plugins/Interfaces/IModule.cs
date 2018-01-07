namespace Artemis.Plugins.Interfaces
{
    public interface IModule : IPlugin
    {
        IPluginViewModel GetMainViewModel();
    }
}