using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Modules;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.Modules
{
    public class ModuleRootViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IModuleVmFactory _moduleVmFactory;

        public ModuleRootViewModel(Module module, IModuleVmFactory moduleVmFactory)
        {
            DisplayName = module?.DisplayName;
            Module = module;

            _moduleVmFactory = moduleVmFactory;
        }

        public Module Module { get; }

        protected override void OnInitialActivate()
        {
            AddTabs();
            base.OnInitialActivate();
        }
        
        private void AddTabs()
        {
            // Create the profile editor and module VMs
            if (Module is ProfileModule profileModule)
                Items.Add(_moduleVmFactory.CreateProfileEditorViewModel(profileModule));

            if (Module.ActivationRequirements.Any())
                Items.Add(_moduleVmFactory.CreateActivationRequirementsViewModel(Module));

            if (Module.ModuleTabs != null)
            {
                List<ModuleTab> moduleTabs = new List<ModuleTab>(Module.ModuleTabs);
                foreach (ModuleTab moduleTab in moduleTabs.Where(m => m != null))
                {
                    ConstructorArgument module = new ConstructorArgument("module", Module);
                    ConstructorArgument displayName = new ConstructorArgument("displayName", DisplayName);

                    ModuleViewModel viewModel = (ModuleViewModel) Module.Plugin.Kernel.Get(moduleTab.Type, module, displayName);
                    Items.Add(viewModel);
                }
            }

            ActiveItem = Items.FirstOrDefault();
        }
    }
}