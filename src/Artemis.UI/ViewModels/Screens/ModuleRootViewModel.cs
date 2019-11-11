using Artemis.Core.Plugins.Abstract;
using Artemis.UI.ViewModels.Controls.ProfileEditor;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class ModuleRootViewModel : Screen
    {
        public ModuleRootViewModel(Module module)
        {
            Module = module;
            ModuleViewModels = new BindableCollection<ModuleViewModel> {new ProfileEditorViewModel(Module)};
            ModuleViewModels.AddRange(Module.GetViewModels());
        }

        public Module Module { get; }
        public BindableCollection<ModuleViewModel> ModuleViewModels { get; set; }

        public int FixedHeaderCount => ModuleViewModels.Count;
    }
}