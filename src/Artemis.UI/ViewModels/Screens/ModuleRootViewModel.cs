using Artemis.Core.Plugins.Abstract;
using Artemis.UI.Ninject.Factories;
using Stylet;

namespace Artemis.UI.ViewModels.Screens
{
    public class ModuleRootViewModel : Conductor<ModuleViewModel>.Collection.OneActive
    {
        public ModuleRootViewModel(Module module, IProfileEditorViewModelFactory profileEditorViewModelFactory)
        {
            Module = module;

            // Add the profile editor and module VMs
            var profileEditor = profileEditorViewModelFactory.CreateModuleViewModel(Module);
            Items.Add(profileEditor);
            Items.AddRange(Module.GetViewModels());

            // Activate the profile editor
            ActiveItem = profileEditor;
        }

        public Module Module { get; }
        
        public int FixedHeaderCount => Items.Count;
    }
}