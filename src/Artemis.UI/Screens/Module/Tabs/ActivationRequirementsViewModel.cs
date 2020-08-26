using System.Linq;
using Artemis.Core.Plugins.Modules;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Module.Tabs
{
    public class ActivationRequirementsViewModel : Conductor<ActivationRequirementViewModel>.Collection.AllActive
    {
        private readonly IModuleVmFactory _moduleVmFactory;

        public ActivationRequirementsViewModel(Core.Plugins.Modules.Module module, IModuleVmFactory moduleVmFactory)
        {
            _moduleVmFactory = moduleVmFactory;

            DisplayName = "Activation requirements";
            Module = module;

            ActivationType = Module.ActivationRequirementMode == ActivationRequirementType.All
                ? "all requirements are met"
                : "any requirement is met";
        }

        public Core.Plugins.Modules.Module Module { get; }

        public string ActivationType { get; set; }
        
        protected override void OnActivate()
        {
            if (!Items.Any())
                Items.AddRange(Module.ActivationRequirements.Select(_moduleVmFactory.CreateActivationRequirementViewModel));

           
        }
    }
}