using System.Collections.ObjectModel;
using Artemis.Core.ScriptingProviders;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that allows you to manage various types of <see cref="Script" /> instances
    /// </summary>
    public interface IScriptingService : IArtemisService
    {
        /// <summary>
        ///     Gets a list of all currently active global scripts
        /// </summary>
        ReadOnlyCollection<GlobalScript> GlobalScripts { get; }

        /// <summary>
        ///     Creates a <see cref="GlobalScript" /> instance for the given <paramref name="scriptConfiguration" />
        /// </summary>
        /// <param name="scriptConfiguration">The script configuration of the script</param>
        /// <returns>
        ///     If the <see cref="ScriptingProvider" /> was found an instance of the script; otherwise <see langword="null" />
        ///     .
        /// </returns>
        GlobalScript? CreateScriptInstance(ScriptConfiguration scriptConfiguration);

        /// <summary>
        ///     Creates a <see cref="ProfileScript" /> instance for the given <paramref name="scriptConfiguration" />
        /// </summary>
        /// <param name="profile">The profile the script configuration is configured for</param>
        /// <param name="scriptConfiguration">The script configuration of the script</param>
        /// <returns>
        ///     If the <see cref="ScriptingProvider" /> was found an instance of the script; otherwise <see langword="null" />
        ///     .
        /// </returns>
        ProfileScript? CreateScriptInstance(Profile profile, ScriptConfiguration scriptConfiguration);

        /// <summary>
        ///     Creates a <see cref="LayerScript" /> instance for the given <paramref name="scriptConfiguration" />
        /// </summary>
        /// <param name="layer">The layer the script configuration is configured fo</param>
        /// <param name="scriptConfiguration">The script configuration of the script</param>
        /// <returns>
        ///     If the <see cref="ScriptingProvider" /> was found an instance of the script; otherwise <see langword="null" />
        ///     .
        /// </returns>
        LayerScript? CreateScriptInstance(Layer layer, ScriptConfiguration scriptConfiguration);

        /// <summary>
        ///     Creates a <see cref="PropertyScript" /> instance for the given <paramref name="scriptConfiguration" />
        /// </summary>
        /// <param name="layerProperty">The layer property the script configuration is configured fo</param>
        /// <param name="scriptConfiguration">The script configuration of the script</param>
        /// <returns>
        ///     If the <see cref="ScriptingProvider" /> was found an instance of the script; otherwise <see langword="null" />
        ///     .
        /// </returns>
        PropertyScript? CreateScriptInstance(ILayerProperty layerProperty, ScriptConfiguration scriptConfiguration);

        /// <summary>
        ///     Deletes the provided global script by it's configuration
        /// </summary>
        void DeleteScript(ScriptConfiguration scriptConfiguration);
    }
}