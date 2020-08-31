using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Serilog;

namespace Artemis.Core.Services
{
    internal class DataBindingService : IDataBindingService
    {
        private readonly ILogger _logger;
        private readonly List<DataBindingModifierType> _registeredDataBindingModifierTypes;

        public DataBindingService(ILogger logger)
        {
            _logger = logger;
            _registeredDataBindingModifierTypes = new List<DataBindingModifierType>();
        }

        public IReadOnlyCollection<DataBindingModifierType> RegisteredDataBindingModifierTypes
        {
            get
            {
                lock (_registeredDataBindingModifierTypes)
                {
                    return _registeredDataBindingModifierTypes.AsReadOnly();
                }
            }
        }

        public void RegisterModifierType(PluginInfo pluginInfo, DataBindingModifierType dataBindingModifierType)
        {
            if (pluginInfo == null)
                throw new ArgumentNullException(nameof(pluginInfo));
            if (dataBindingModifierType == null)
                throw new ArgumentNullException(nameof(dataBindingModifierType));

            lock (_registeredDataBindingModifierTypes)
            {
                if (_registeredDataBindingModifierTypes.Contains(dataBindingModifierType))
                    return;

                dataBindingModifierType.Register(pluginInfo, this);
                _registeredDataBindingModifierTypes.Add(dataBindingModifierType);
            }
        }

        public void RemoveModifierType(DataBindingModifierType dataBindingModifierType)
        {
            if (dataBindingModifierType == null)
                throw new ArgumentNullException(nameof(dataBindingModifierType));

            lock (_registeredDataBindingModifierTypes)
            {
                if (!_registeredDataBindingModifierTypes.Contains(dataBindingModifierType))
                    return;

                dataBindingModifierType.Unsubscribe();
                _registeredDataBindingModifierTypes.Remove(dataBindingModifierType);
            }
        }

        public List<DataBindingModifierType> GetCompatibleModifierTypes(Type type)
        {
            lock (_registeredDataBindingModifierTypes)
            {
                if (type == null)
                    return new List<DataBindingModifierType>(_registeredDataBindingModifierTypes);

                var candidates = _registeredDataBindingModifierTypes.Where(c => c.CompatibleTypes.Any(t => t.IsCastableFrom(type))).ToList();

                // If there are multiple modifier types with the same description, use the closest match
                foreach (var dataBindingModifierTypes in candidates.GroupBy(c => c.Description).Where(g => g.Count() > 1).ToList())
                {
                    var bestCandidate = dataBindingModifierTypes.OrderByDescending(c => c.CompatibleTypes.Contains(type)).FirstOrDefault();
                    foreach (var dataBindingModifierType in dataBindingModifierTypes)
                    {
                        if (dataBindingModifierType != bestCandidate)
                            candidates.Remove(dataBindingModifierType);
                    }
                }

                return candidates;
            }
        }

        public DataBindingModifierType GetModifierType(Guid modifierTypePluginGuid, string modifierType)
        {
            return RegisteredDataBindingModifierTypes.FirstOrDefault(o => o.PluginInfo.Guid == modifierTypePluginGuid && o.GetType().Name == modifierType);
        }

        public void LogModifierDeserializationFailure(DataBindingModifier dataBindingModifier, JsonSerializationException exception)
        {
            _logger.Warning(
                exception,
                "Failed to deserialize static parameter for operator {order}. {operatorType}",
                dataBindingModifier.Entity.Order,
                dataBindingModifier.Entity.ModifierType
            );
        }
    }
}