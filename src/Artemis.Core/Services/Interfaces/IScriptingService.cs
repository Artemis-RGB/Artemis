using System.Collections.ObjectModel;
using Artemis.Core.ScriptingProviders;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that allows you to manage various types of <see cref="NodeScript" /> instances
    /// </summary>
    public interface IScriptingService : IArtemisService
    {
        /// <summary>
        ///     Gets a list of all available scripting providers
        /// </summary>
        ReadOnlyCollection<ScriptingProvider> ScriptingProviders { get; }

        /// <summary>
        ///     Gets a list of all currently active global scripts
        /// </summary>
        ReadOnlyCollection<GlobalScript> GlobalScripts { get; }

        /// <summary>
        ///     Adds a script by the provided script configuration to the provided profile and instantiates it.
        /// </summary>
        /// <param name="scriptConfiguration">The script configuration whose script to add.</param>
        /// <param name="profile">The profile to add the script to.</param>
        ProfileScript AddScript(ScriptConfiguration scriptConfiguration, Profile profile);

        /// <summary>
        ///     Removes a script by the provided script configuration from the provided profile and disposes it.
        /// </summary>
        /// <param name="scriptConfiguration">The script configuration whose script to remove.</param>
        /// <param name="profile">The profile to remove the script from.</param>
        void RemoveScript(ScriptConfiguration scriptConfiguration, Profile profile);

        /// <summary>
        ///     Adds a script by the provided script configuration to the global collection and instantiates it.
        /// </summary>
        /// <param name="scriptConfiguration">The script configuration whose script to add.</param>
        GlobalScript AddScript(ScriptConfiguration scriptConfiguration);

        /// <summary>
        ///     Removes a script by the provided script configuration from the global collection and disposes it.
        /// </summary>
        /// <param name="scriptConfiguration">The script configuration whose script to remove.</param>
        void RemoveScript(ScriptConfiguration scriptConfiguration);
    }
}