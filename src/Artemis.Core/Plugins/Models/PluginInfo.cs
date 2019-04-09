using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Interfaces;
using Newtonsoft.Json;
using Ninject;

namespace Artemis.Core.Plugins.Models
{
    public class PluginInfo
    {
        private static AppDomain _appDomain;
        
        /// <summary>
        ///     The plugin's GUID
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     The name of the plugin
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The version of the plugin
        /// </summary>
        public string Version { get; set; }
        
        /// <summary>
        ///     The instantiated plugin, available after successful load
        /// </summary>
        [JsonIgnore]
        public IPlugin Plugin { get; set; }

        /// <summary>
        ///     Full path to the plugin's current folder
        /// </summary>
        [JsonIgnore]
        public string Folder { get; set; }
        
        /// <summary>
        ///     Gets the view model of the module accompanying the provided plugin info
        /// </summary>
        /// <param name="kernel">The Ninject kernel to use for DI</param>
        /// <returns></returns>
        public IModuleViewModel GetModuleViewModel(IKernel kernel)
        {
            // Don't attempt to locate VMs for something other than a module
            if (Plugin == null)
                throw new ArtemisPluginException(this, "Cannot locate a view model for this plugin because it's not compiled.");
            if (!(Plugin is IModule module))
                throw new ArtemisPluginException(this, "Cannot locate a view model for this plugin as it's not a module.");

            // Get the type from the module
            var vmType = module.ViewModelType;
            if (!typeof(IModuleViewModel).IsAssignableFrom(vmType))
                throw new ArtemisPluginException(this, "ViewModel must implement IModuleViewModel.");

            // Instantiate the ViewModel with Ninject
            var vm = (IModuleViewModel) kernel.Get(vmType);
            vm.PluginInfo = this;
            return vm;
        }

        public override string ToString()
        {
            return $"{nameof(Guid)}: {Guid}, {nameof(Name)}: {Name}, {nameof(Version)}: {Version}";
        }
    }
}