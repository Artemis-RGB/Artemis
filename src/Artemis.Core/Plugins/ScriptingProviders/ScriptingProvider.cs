using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Artemis.Core.ScriptingProviders
{
    /// <summary>
    ///     Allows you to implement and register your own scripting provider.
    /// </summary>
    public abstract class ScriptingProvider<TGlobalScript, TProfileScript, TLayerScript, TPropertyScript> : ScriptingProvider
        where TGlobalScript : GlobalScript
        where TProfileScript : ProfileScript
        where TLayerScript : LayerScript
        where TPropertyScript : PropertyScript
    {
        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="GlobalScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreateGlobalScriptEditor(TGlobalScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="ProfileScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreateProfileScriptEditor(TProfileScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="LayerScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreateLayerScriptScriptEditor(TLayerScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="PropertyScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreatePropertyScriptEditor(TPropertyScript script);

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
        internal override Type GlobalScriptType => typeof(TGlobalScript);

        /// <inheritdoc />
        internal override Type ProfileScriptType => typeof(TProfileScript);

        /// <inheritdoc />
        internal override Type LayerScriptType => typeof(TLayerScript);

        /// <inheritdoc />
        internal override Type PropertyScriptType => typeof(TPropertyScript);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="GlobalScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public override IScriptEditorViewModel CreateGlobalScriptEditor(GlobalScript script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            if (script.GetType() != GlobalScriptType)
                throw new ArtemisCoreException($"This scripting provider only supports global scripts of type {GlobalScriptType.Name}");

            return CreateGlobalScriptEditor((TGlobalScript) script);
        }

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="ProfileScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public override IScriptEditorViewModel CreateProfileScriptEditor(ProfileScript script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            if (script.GetType() != ProfileScriptType)
                throw new ArtemisCoreException($"This scripting provider only supports profile scripts of type {ProfileScriptType.Name}");

            return CreateProfileScriptEditor((TProfileScript) script);
        }

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="LayerScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public override IScriptEditorViewModel CreateLayerScriptScriptEditor(LayerScript script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            if (script.GetType() != LayerScriptType)
                throw new ArtemisCoreException($"This scripting provider only supports layer scripts of type {LayerScriptType.Name}");

            return CreateLayerScriptScriptEditor((TLayerScript) script);
        }

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="PropertyScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public override IScriptEditorViewModel CreatePropertyScriptEditor(PropertyScript script)
        {
            if (script == null) throw new ArgumentNullException(nameof(script));
            if (script.GetType() != PropertyScriptType)
                throw new ArtemisCoreException($"This scripting provider only supports property scripts of type {PropertyScriptType.Name}");

            return CreatePropertyScriptEditor((TPropertyScript) script);
        }

        #endregion
    }

    /// <summary>
    ///     Allows you to implement and register your own scripting provider.
    ///     <para>
    ///         Note: You can't implement this, implement
    ///         <see cref="ScriptingProvider{TProfileScript,TLayerScript,TPropertyScript,TGlobalScript}" /> instead.
    ///     </para>
    /// </summary>
    public abstract class ScriptingProvider : PluginFeature
    {
        /// <summary>
        ///     Gets the name of the scripting language this provider provides
        /// </summary>
        public abstract string LanguageName { get; }

        /// <summary>
        ///     Gets a list of all active scripts belonging to this scripting provider
        /// </summary>
        public ReadOnlyCollection<Script> Scripts => InternalScripts.AsReadOnly();

        internal abstract Type GlobalScriptType { get; }
        internal abstract Type PropertyScriptType { get; }
        internal abstract Type LayerScriptType { get; }
        internal abstract Type ProfileScriptType { get; }
        internal List<Script> InternalScripts { get; } = new();

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="GlobalScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreateGlobalScriptEditor(GlobalScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="ProfileScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreateProfileScriptEditor(ProfileScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="LayerScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreateLayerScriptScriptEditor(LayerScript script);

        /// <summary>
        ///     Called when the UI needs a script editor for a <see cref="PropertyScript" />
        /// </summary>
        /// <param name="script">The script the editor must edit</param>
        public abstract IScriptEditorViewModel CreatePropertyScriptEditor(PropertyScript script);
    }

    /// <summary>
    ///     Represents a view model containing a script editor
    /// </summary>
    public interface IScriptEditorViewModel
    {
        /// <summary>
        ///     Gets the script this editor is editing
        /// </summary>
        Script Script { get; }
    }
}