using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core.Modules;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Sidebar;

public class ModuleActivationRequirementsViewModel : ViewModelBase
{
    public ModuleActivationRequirementsViewModel(Module module)
    {
        ActivationType = module.ActivationRequirementMode == ActivationRequirementType.All ? "all requirements are met" : "any requirements are met";
        ActivationRequirements = module.ActivationRequirements != null
            ? new ObservableCollection<ModuleActivationRequirementViewModel>(module.ActivationRequirements.Select(r => new ModuleActivationRequirementViewModel(r)))
            : new ObservableCollection<ModuleActivationRequirementViewModel>();
    }

    public string ActivationType { get; }
    public ObservableCollection<ModuleActivationRequirementViewModel> ActivationRequirements { get; }
}