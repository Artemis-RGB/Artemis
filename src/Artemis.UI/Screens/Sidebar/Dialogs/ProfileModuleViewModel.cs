using Artemis.Core.Modules;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileModuleViewModel : PropertyChangedBase
    {
        private bool _isSelected;

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

        public bool IsSelected
        {
            get => _isSelected;
            set => SetAndNotify(ref _isSelected, value);
        }

        public Module Module { get; }
    }
}