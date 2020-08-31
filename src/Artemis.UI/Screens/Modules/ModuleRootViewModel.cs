using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.UI.Ninject.Factories;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Screens.Modules
{
    public class ModuleRootViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IKernel _kernel;
        private readonly IModuleVmFactory _moduleVmFactory;

        public ModuleRootViewModel(Module module, IModuleVmFactory moduleVmFactory, IKernel kernel)
        {
            DisplayName = module?.DisplayName;
            Module = module;

            _moduleVmFactory = moduleVmFactory;
            _kernel = kernel;
        }

        public Module Module { get; }

        protected override void OnActivate()
        {
            AddTabs();
            base.OnActivate();
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