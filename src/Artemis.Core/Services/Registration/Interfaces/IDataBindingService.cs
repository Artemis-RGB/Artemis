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
        /// <param name="plugin">The plugin this modifier type belongs to</param>
        /// <param name="dataBindingModifierType">The modifier type to register</param>
        DataBindingModifierTypeRegistration RegisterModifierType([NotNull] Plugin plugin, [NotNull] BaseDataBindingModifierType dataBindingModifierType);

        /// <summary>
        ///     Removes a modifier type so it is no longer available for use in data bindings
        /// </summary>
        /// <param name="dataBindingModifierType">The registration of the modifier type to remove</param>
        void RemoveModifierType([NotNull] DataBindingModifierTypeRegistration dataBindingModifierType);

        /// <summary>
        ///     Returns all the data binding modifier types compatible with the provided type
        /// </summary>
        List<BaseDataBindingModifierType> GetCompatibleModifierTypes(Type type, ModifierTypePart part);

        /// <summary>
        ///     Gets a modifier type by its plugin GUID and type name
        /// </summary>
        /// <param name="modifierTypePluginGuid">The modifier type's plugin GUID</param>
        /// <param name="modifierType">The type name of the modifier type</param>
        /// <returns></returns>
        BaseDataBindingModifierType GetModifierType(Guid modifierTypePluginGuid, string modifierType);
    }
}