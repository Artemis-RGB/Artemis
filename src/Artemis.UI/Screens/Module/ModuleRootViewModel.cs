using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.Screens.Module
{
    public class ModuleRootViewModel : Conductor<ModuleViewModel>.Collection.OneActive
    {
        public ModuleRootViewModel(Core.Plugins.Abstract.Module module, IProfileEditorViewModelFactory profileEditorViewModelFactory)
        {
            Module = module;

            // Add the profile editor and module VMs
            var profileEditor = profileEditorViewModelFactory.CreateModuleViewModel(Module);
            Items.Add(profileEditor);
            Items.AddRange(Module.GetViewModels());

            // Activate the profile editor
            ActiveItem = profileEditor;
        }

        public string Title => Module?.DisplayName;
        public Core.Plugins.Abstract.Module Module { get; }
        public int FixedHeaderCount => Items.Count;
    }
}