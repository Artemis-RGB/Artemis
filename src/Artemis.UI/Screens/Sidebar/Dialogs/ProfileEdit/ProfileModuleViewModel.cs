using Artemis.Core.Modules;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit
{
    public class ProfileModuleViewModel : PropertyChangedBase
    {
        public ProfileModuleViewModel(Module module)
        {
            Module = module;
            Name = module.DisplayName;
            if (module.DisplayIcon != null)
                Icon = module.DisplayIcon.EndsWith(".svg") ? module.Plugin.ResolveRelativePath(module.DisplayIcon) : module.DisplayIcon;
            else
                Icon = PackIconKind.QuestionMark.ToString();

            Description = module.Info.Description;
        }

        public string Icon { get; }
        public string Name { get; }
        public string Description { get; }

        public Module Module { get; }
    }
}