using Artemis.Core.Modules;
using Artemis.UI.Shared;
using Material.Icons;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileModuleViewModel : ViewModelBase
    {
        public ProfileModuleViewModel(Module module)
        {
            Module = module;
            Name = module.Info.Name;
            Icon = module.Info.ResolvedIcon ?? MaterialIconKind.QuestionMark.ToString();
            Description = module.Info.Description;
        }

        public string Icon { get; }
        public string Name { get; }
        public string? Description { get; }

        public Module Module { get; }
    }
}