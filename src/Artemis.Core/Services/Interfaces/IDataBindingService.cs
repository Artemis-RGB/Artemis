using System;
using System.Collections.Generic;
using Artemis.Core.Properties;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    public interface IDataBindingService : IArtemisService
    {
        /// <summary>
        ///     Gets a read-only collection of all registered modifier types
        /// </summary>
        IReadOnlyCollection<DataBindingModifierType> RegisteredDataBindingModifierTypes { get; }

        /// <summary>
        ///     Registers a new modifier type for use in data bindings
        /// </summary>
        /// <param name="pluginInfo">The PluginInfo of the plugin this modifier type belongs to</param>
        /// <param name="dataBindingModifierType">The modifier type to register</param>
        void RegisterModifierType([NotNull] PluginInfo pluginInfo, [NotNull] DataBindingModifierType dataBindingModifierType);

        /// <summary>
        ///     Removes a modifier type so it is no longer available for use in data bindings
        /// </summary>
        /// <param name="dataBindingModifierType">The modifier type to remove</param>
        void RemoveModifierType([NotNull] DataBindingModifierType dataBindingModifierType);

        /// <summary>
        ///     Returns all the data binding modifier types compatible with the provided type
        /// </summary>
        List<DataBindingModifierType> GetCompatibleModifierTypes(Type type);

        /// <summary>
        ///     Gets a modifier type by its plugin GUID and type name
        /// </summary>
        /// <param name="modifierTypePluginGuid">The modifier type's plugin GUID</param>
        /// <param name="modifierType">The type name of the modifier type</param>
        /// <returns></returns>
        DataBindingModifierType GetModifierType(Guid modifierTypePluginGuid, string modifierType);

        /// <summary>
        ///     Logs a modifier deserialization failure
        /// </summary>
        /// <param name="modifierName">The modifier that failed to deserialize</param>
        /// <param name="exception">The JSON exception that occurred</param>
        void LogModifierDeserializationFailure(string modifierName, JsonSerializationException exception);
    }
}