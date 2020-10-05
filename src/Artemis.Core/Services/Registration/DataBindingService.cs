using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DefaultTypes;

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
            // Numbers - General
            RegisterModifierType(Constants.CorePluginInfo, new SumModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new SubtractModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new MultiplicationModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new DivideModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new PercentageOfModifierType());

            // Numbers - Advanced
            RegisterModifierType(Constants.CorePluginInfo, new MaxModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new MinModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new ModuloModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new AbsoluteModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new PowerModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new SquareRootModifierType());
            
            // Numbers - Rounding
            RegisterModifierType(Constants.CorePluginInfo, new FloorModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new RoundModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new CeilingModifierType());

            // Numbers - Trigonometric
            RegisterModifierType(Constants.CorePluginInfo, new SineModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new CosineModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new TangentModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new CotangentModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new SecantModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new CosecantModifierType());

            // Colors
            RegisterModifierType(Constants.CorePluginInfo, new SKColorSumModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new SKColorBrightenModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new SKColorDarkenModifierType());
            RegisterModifierType(Constants.CorePluginInfo, new SKColorRotateHueModifierType());
        }
    }
}