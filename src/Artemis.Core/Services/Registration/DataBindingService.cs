using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core.Services
{
    internal class DataBindingService : IDataBindingService
    {
        public DataBindingService()
        {
            RegisterBuiltInModifiers();
        }

        public DataBindingModifierTypeRegistration RegisterModifierType(PluginInfo pluginInfo, DataBindingModifierType dataBindingModifierType)
        {
            if (pluginInfo == null)
                throw new ArgumentNullException(nameof(pluginInfo));
            if (dataBindingModifierType == null)
                throw new ArgumentNullException(nameof(dataBindingModifierType));

            dataBindingModifierType.PluginInfo = pluginInfo;
            return DataBindingModifierTypeStore.Add(dataBindingModifierType);
        }

        public void RemoveModifierType(DataBindingModifierTypeRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            DataBindingModifierTypeStore.Remove(registration);
        }

        public List<DataBindingModifierType> GetCompatibleModifierTypes(Type type)
        {
            return DataBindingModifierTypeStore.GetForType(type).Select(r => r.DataBindingModifierType).ToList();
        }

        public DataBindingModifierType GetModifierType(Guid modifierTypePluginGuid, string modifierType)
        {
            return DataBindingModifierTypeStore.Get(modifierTypePluginGuid, modifierType)?.DataBindingModifierType;
        }

        private void RegisterBuiltInModifiers()
        {
            RegisterModifierType(Constants.CorePluginInfo, new MultiplicationModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new DivideModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new FloorModifierType());
        }
    }
}