using System;

namespace Artemis.Core.Modules
{
    /// <inheritdoc />
    public class ModuleTab<T> : ModuleTab where T : ModuleViewModel
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleTab{T}" /> class
        /// </summary>
        /// <param name="title">The title of the tab</param>
        public ModuleTab(string title)
        {
            Title = title;
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
        ///     The module this tab belongs to
        /// </summary>
        internal Module Module { get; set; }

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