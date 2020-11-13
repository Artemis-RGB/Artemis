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

        public DataBindingModifierTypeRegistration RegisterModifierType(Plugin plugin, BaseDataBindingModifierType dataBindingModifierType)
        {
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (dataBindingModifierType == null)
                throw new ArgumentNullException(nameof(dataBindingModifierType));

            dataBindingModifierType.Plugin = plugin;
            return DataBindingModifierTypeStore.Add(dataBindingModifierType);
        }

        public void RemoveModifierType(DataBindingModifierTypeRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));
            DataBindingModifierTypeStore.Remove(registration);
        }

        public List<BaseDataBindingModifierType> GetCompatibleModifierTypes(Type type, ModifierTypePart part)
        {
            return DataBindingModifierTypeStore.GetForType(type, part).Select(r => r.DataBindingModifierType).ToList();
        }

        public BaseDataBindingModifierType GetModifierType(Guid modifierTypePluginGuid, string modifierType)
        {
            return DataBindingModifierTypeStore.Get(modifierTypePluginGuid, modifierType)?.DataBindingModifierType;
        }

        private void RegisterBuiltInModifiers()
        {
            // Numbers - General
            RegisterModifierType(Constants.CorePlugin, new SumModifierType());
            RegisterModifierType(Constants.CorePlugin, new SubtractModifierType());
            RegisterModifierType(Constants.CorePlugin, new MultiplicationModifierType());
            RegisterModifierType(Constants.CorePlugin, new DivideModifierType());
            RegisterModifierType(Constants.CorePlugin, new PercentageOfModifierType());

            // Numbers - Advanced
            RegisterModifierType(Constants.CorePlugin, new MaxModifierType());
            RegisterModifierType(Constants.CorePlugin, new MinModifierType());
            RegisterModifierType(Constants.CorePlugin, new ModuloModifierType());
            RegisterModifierType(Constants.CorePlugin, new AbsoluteModifierType());
            RegisterModifierType(Constants.CorePlugin, new PowerModifierType());
            RegisterModifierType(Constants.CorePlugin, new SquareRootModifierType());
            
            // Numbers - Rounding
            RegisterModifierType(Constants.CorePlugin, new FloorModifierType());
            RegisterModifierType(Constants.CorePlugin, new RoundModifierType());
            RegisterModifierType(Constants.CorePlugin, new CeilingModifierType());

            // Numbers - Trigonometric
            RegisterModifierType(Constants.CorePlugin, new SineModifierType());
            RegisterModifierType(Constants.CorePlugin, new CosineModifierType());
            RegisterModifierType(Constants.CorePlugin, new TangentModifierType());
            RegisterModifierType(Constants.CorePlugin, new CotangentModifierType());
            RegisterModifierType(Constants.CorePlugin, new SecantModifierType());
            RegisterModifierType(Constants.CorePlugin, new CosecantModifierType());

            // Colors
            RegisterModifierType(Constants.CorePlugin, new SKColorSumModifierType());
            RegisterModifierType(Constants.CorePlugin, new SKColorSaturateModifierType());
            RegisterModifierType(Constants.CorePlugin, new SKColorDesaturateModifierType());
            RegisterModifierType(Constants.CorePlugin, new SKColorBrightenModifierType());
            RegisterModifierType(Constants.CorePlugin, new SKColorDarkenModifierType());
            RegisterModifierType(Constants.CorePlugin, new SKColorRotateHueModifierType());
            RegisterModifierType(Constants.CorePlugin, new SKColorInvertModifierType());
        }
    }
}