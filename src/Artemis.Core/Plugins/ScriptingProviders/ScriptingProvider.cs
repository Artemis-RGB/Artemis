using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Allows you to implement and register your own scripting provider.
    /// </summary>
    public abstract class ScriptingProvider<TGlobalScript, TProfileScript> : ScriptingProvider
        where TGlobalScript : GlobalScript
        where TProfileScript : ProfileScript
    {
        #region Overrides of PluginFeature

        /// <inheritdoc />
        internal override void InternalDisable()
        {
            base.InternalDisable();

            while (Scripts.Count > 0)
                Scripts[0].Dispose();
        }

        #endregion

        #region Overrides of ScriptingProvider

        /// <inheritdoc />
        internal override Type ProfileScriptType => typeof(TProfileScript);

        /// <inheritdoc />
        internal override Type GlobalScriptType => typeof(TGlobalScript);

        #endregion
    }

    /// <summary>
    ///     Allows you to implement and register your own scripting provider.
    ///     <para>
    ///         Note: You can't implement this, implement
    ///         <see cref="ScriptingProvider{TGlobalScript, TProfileScript}" /> instead.
    ///     </para>
    /// </summary>
    public abstract class ScriptingProvider : PluginFeature
    {
        /// <summary>
        ///     The base constructor of the <see cref="ScriptingProvider" /> class
        /// </summary>
        protected ScriptingProvider()
        {
            Scripts = new ReadOnlyCollection<Script>(InternalScripts);
        }

        /// <summary>
        ///     Gets the name of the scripting language this provider provides
        /// </summary>
        public abstract string LanguageName { get; }

        /// <summary>
        ///     Gets a list of all active scripts belonging to this scripting provider
        /// </summary>
        public ReadOnlyCollection<Script> Scripts { get; }

        internal abstract Type GlobalScriptType { get; }
        internal abstract Type ProfileScriptType { get; }
        internal List<Script> InternalScripts { get; } = new();

        /// <summary>
        ///     Called when the UI needs a script editor for the specified <paramref name="scriptType" />
        /// </summary>
        /// <param name="scriptType">The type of script the editor will host</param>
        public abstract IScriptEditorViewModel CreateScriptEditor(ScriptType scriptType);

        /// <summary>
        /// Called when a script for a certain type needs default content.
        /// </summary>
        /// <param name="scriptType">The type of script the default content is for.</param>
        public abstract string GetDefaultScriptContent(ScriptType scriptType);
    }
}