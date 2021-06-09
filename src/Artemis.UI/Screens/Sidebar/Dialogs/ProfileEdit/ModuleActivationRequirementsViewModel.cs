using System.Linq;
using Artemis.Core.Modules;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit
{
    public class ModuleActivationRequirementsViewModel : Conductor<ModuleActivationRequirementViewModel>.Collection.AllActive
    {
        private readonly ISidebarVmFactory _sidebarVmFactory;
        private Module _module;
        private string _activationType;

        public ModuleActivationRequirementsViewModel(ISidebarVmFactory sidebarVmFactory)
        {
            _sidebarVmFactory = sidebarVmFactory;
        }

        public Module Module
        {
            get => _module;
            set => SetAndNotify(ref _module, value);
        }

        public string ActivationType
        {
            get => _activationType;
            set => SetAndNotify(ref _activationType, value);
        }

        public void SetModule(Module module)
        {
            Module = module;
            ActivationType = Module != null && Module.ActivationRequirementMode == ActivationRequirementType.All
                ? "all requirements are met"
                : "any requirement is met";

            Items.Clear();
            if (Module?.ActivationRequirements != null)
                Items.AddRange(Module.ActivationRequirements.Select(_sidebarVmFactory.ModuleActivationRequirementViewModel));
        }
    }
}