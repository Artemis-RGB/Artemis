using System.Threading.Tasks;

namespace Artemis.Plugins.Interfaces
{
    public interface IPlugin
    {
        /// <summary>
        ///     Called when the plugin is loaded
        /// </summary>
        void LoadPlugin();

        /// <summary>
        ///     Called when the plugin is unloaded
        /// </summary>
        void UnloadPlugin();
    }
}