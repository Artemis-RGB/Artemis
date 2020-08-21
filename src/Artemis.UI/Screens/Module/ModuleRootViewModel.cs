using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Modules;
using Artemis.UI.Ninject.Factories;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.Module
{
    public class ModuleRootViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IProfileEditorVmFactory _profileEditorVmFactory;
        private readonly IKernel _kernel;

        public ModuleRootViewModel(Core.Plugins.Modules.Module module, IProfileEditorVmFactory profileEditorVmFactory, IKernel kernel)
        {
            DisplayName = module?.DisplayName;
            Module = module;

            _profileEditorVmFactory = profileEditorVmFactory;
            _kernel = kernel;

            Task.Run(AddTabsAsync);
        }

        public Core.Plugins.Modules.Module Module { get; }

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