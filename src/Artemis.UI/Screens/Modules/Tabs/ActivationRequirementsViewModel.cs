using System.Linq;
using Artemis.Core.Modules;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Modules.Tabs
{
    public class ActivationRequirementsViewModel : Conductor<ActivationRequirementViewModel>.Collection.AllActive
    {
        private readonly IModuleVmFactory _moduleVmFactory;

        public ActivationRequirementsViewModel(Module module, IModuleVmFactory moduleVmFactory)
        {
            _moduleVmFactory = moduleVmFactory;

            DisplayName = "ACTIVATION REQUIREMENTS";
            Module = module;

            ActivationType = Module.ActivationRequirementMode == ActivationRequirementType.All
                ? "all requirements are met"
                : "any requirement is met";
        }

        public Module Module { get; }

        public string ActivationType { get; set; }

        protected override void OnActivate()
        {
            Items.Clear();
            Items.AddRange(Module.ActivationRequirements.Select(_moduleVmFactory.CreateActivationRequirementViewModel));
        }
    }
}