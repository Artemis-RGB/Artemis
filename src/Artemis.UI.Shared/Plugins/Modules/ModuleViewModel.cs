using Artemis.Core.Modules;
using Stylet;

namespace Artemis.UI.Shared.Modules
{
    /// <summary>
    ///     The base class for any view model that belongs to a module
    /// </summary>
    public abstract class ModuleViewModel : Screen, IModuleViewModel
    {
        /// <summary>
        ///     The base class for any view model that belongs to a module
        /// </summary>
        /// <param name="module">The module this view model belongs to</param>
        /// <param name="displayName">The name of the tab that's shown on the modules UI page</param>
        protected ModuleViewModel(Module module, string displayName)
        {
            Module = module;
            DisplayName = displayName.ToUpper();
        }

        /// <summary>
        ///     Gets the module this view model belongs to
        /// </summary>
        public Module Module { get; }
    }
}