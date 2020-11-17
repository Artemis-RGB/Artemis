using System;

namespace Artemis.Core.Modules
{
    /// <inheritdoc />
    public class ModuleTab<T> : ModuleTab where T : IModuleViewModel
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ModuleTab{T}" /> class
        /// </summary>
        /// <param name="title">The title of the tab</param>
        public ModuleTab(string title) : base(title)
        {
        }

        /// <inheritdoc />
        public override Type Type => typeof(T);
    }

    /// <summary>
    ///     Describes a UI tab for a specific module
    /// </summary>
    public abstract class ModuleTab
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ModuleTab" /> class
        /// </summary>
        /// <param name="title">The title of the tab</param>
        protected ModuleTab(string title)
        {
            Title = title;
        }

        /// <summary>
        ///     The title of the tab
        /// </summary>
        public string Title { get; protected set; }

        /// <summary>
        ///     The type of view model the tab contains
        /// </summary>
        public abstract Type Type { get; }
    }
}