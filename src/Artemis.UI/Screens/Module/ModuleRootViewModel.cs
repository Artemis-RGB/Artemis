using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Modules;
using Artemis.Core.Services;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject.Factories;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.Module
{
    public class ModuleRootViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IModuleService _moduleService;
        private readonly IProfileEditorVmFactory _profileEditorVmFactory;
        private readonly IKernel _kernel;

        public ModuleRootViewModel(Core.Plugins.Modules.Module module, IModuleService moduleService, IProfileEditorVmFactory profileEditorVmFactory, IKernel kernel)
        {
            DisplayName = module?.DisplayName;
            Module = module;

            _moduleService = moduleService;
            _profileEditorVmFactory = profileEditorVmFactory;
            _kernel = kernel;
        }

        public Core.Plugins.Modules.Module Module { get; }

        protected override void OnActivate()
        {
            Task.Run(async () =>
            {
                await _moduleService.SetActiveModuleOverride(Module);
                await AddTabsAsync();
            });
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            Task.Run(async () =>
            {
                await _moduleService.SetActiveModuleOverride(null);
            });
            base.OnDeactivate();
        }

        private async Task AddTabsAsync()
        {
            // Give the screen a moment to active without freezing the UI thread
            await Task.Delay(400);

            // Create the profile editor and module VMs
            if (Module is ProfileModule profileModule)
            {
                var profileEditor = _profileEditorVmFactory.Create(profileModule);
                Items.Add(profileEditor);
            }

            if (Module.ModuleTabs != null)
            {
                var moduleTabs = new List<ModuleTab>(Module.ModuleTabs);
                foreach (var moduleTab in moduleTabs.Where(m => m != null))
                {
                    var module = new ConstructorArgument("module", Module);
                    var displayName = new ConstructorArgument("displayName", DisplayName);

                    var viewModel = (ModuleViewModel) _kernel.Get(moduleTab.Type, module, displayName);
                    Items.Add(viewModel);
                }
            }

            ActiveItem = Items.FirstOrDefault();
        }
    }
}