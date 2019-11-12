using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.ViewModels.Controls.ProfileEditor;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class ModuleRootViewModel : Screen
    {
        public ModuleRootViewModel(Module module, IProfileEditorViewModelFactory profileEditorViewModelFactory)
        {
            Module = module;
            ModuleViewModels = new BindableCollection<ModuleViewModel> {profileEditorViewModelFactory.CreateModuleViewModel(Module)};
            ModuleViewModels.AddRange(Module.GetViewModels());
        }

        public Module Module { get; }
        public BindableCollection<ModuleViewModel> ModuleViewModels { get; set; }

        public int FixedHeaderCount => ModuleViewModels.Count;
    }
}