using System;
using System.Collections.Generic;
using Artemis.Core.Properties;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service that allows you to register and retrieve data binding modifiers used by the data bindings system
    /// </summary>
    public interface IDataBindingService : IArtemisService
    {
        /// <summary>
        ///     Registers a new modifier type for use in data bindings
        /// </summary>
        /// <param name="pluginInfo">The PluginInfo of the plugin this modifier type belongs to</param>
        /// <param name="dataBindingModifierType">The modifier type to register</param>
        DataBindingModifierTypeRegistration RegisterModifierType([NotNull] PluginInfo pluginInfo, [NotNull] DataBindingModifierType dataBindingModifierType);

        /// <summary>
        ///     Removes a modifier type so it is no longer available for use in data bindings
        /// </summary>
        /// <param name="dataBindingModifierType">The registration of the modifier type to remove</param>
        void RemoveModifierType([NotNull] DataBindingModifierTypeRegistration dataBindingModifierType);

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
    }
}