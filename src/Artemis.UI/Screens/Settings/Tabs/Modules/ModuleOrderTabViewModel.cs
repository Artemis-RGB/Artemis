using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Modules;
using Artemis.Core.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Settings.Tabs.Modules
{
    public class ModuleOrderTabViewModel : Screen
    {
        private readonly IPluginService _pluginService;

        public ModuleOrderTabViewModel(IPluginService pluginService)
        {
            DisplayName = "MODULE PRIORITY";

            _pluginService = pluginService;
            NormalModules = new BindableCollection<Core.Plugins.Modules.Module>();
            ApplicationModules = new BindableCollection<Core.Plugins.Modules.Module>();
            OverlayModules = new BindableCollection<Core.Plugins.Modules.Module>();
        }

        public BindableCollection<Core.Plugins.Modules.Module> NormalModules { get; set; }
        public BindableCollection<Core.Plugins.Modules.Module> ApplicationModules { get; set; }
        public BindableCollection<Core.Plugins.Modules.Module> OverlayModules { get; set; }

        protected override void OnActivate()
        {
            // Take it off the UI thread to avoid freezing on tab change
            Task.Run(() =>
            {
                NormalModules.Clear();
                ApplicationModules.Clear();
                OverlayModules.Clear();

                var instances = _pluginService.GetPluginsOfType<Core.Plugins.Modules.Module>().ToList();
                foreach (var module in instances)
                {
                    if (module.PriorityCategory == ModulePriorityCategory.Normal)
                        NormalModules.Add(module);
                    else if (module.PriorityCategory == ModulePriorityCategory.Application)
                        ApplicationModules.Add(module);
                    else if (module.PriorityCategory == ModulePriorityCategory.Overlay)
                        OverlayModules.Add(module);
                }
            });
        }
    }
}