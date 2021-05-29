using Artemis.Core.Modules;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileModuleViewModel : PropertyChangedBase
    {
        public ProfileModuleViewModel(Module module)
        {
            Module = module;
            Icon = module.DisplayIcon;
            Name = module.DisplayName;
            Description = module.Info.Description;
        }

        public string Icon { get; }
        public string Name { get; }
        public string Description { get; }

        public Module Module { get; }
    }
}