using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.Exceptions;
using Artemis.Core.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;

namespace Artemis.Core.Services
{
    public class MainDataModelService : IMainDataModelService
    {
        private readonly List<IDataModelExpansion> _dataModelExpansions;

        public MainDataModelService()
        {
            _dataModelExpansions = new List<IDataModelExpansion>();
        }

        public ReadOnlyCollection<IDataModelExpansion> DataModelExpansions
        {
            get
            {
                lock (_dataModelExpansions)
                {
                    return _dataModelExpansions.AsReadOnly();
                }
            }
        }

        public void Update(double deltaTime)
        {
            lock (_dataModelExpansions)
            {
                // Update all expansions
                foreach (var expansion in _dataModelExpansions)
                    expansion.Update(deltaTime);
            }
        }

        public void AddExpansion(IDataModelExpansion dataModelExpansion)
        {
            lock (_dataModelExpansions)
            {
                _dataModelExpansions.Add(dataModelExpansion);
                // TODO SpoinkyNL 3-3-2018: Initialize the expansion and fire an event
            }
        }

        public void RemoveExpansion(IDataModelExpansion dataModelExpansion)
        {
            lock (_dataModelExpansions)
            {
                if (!_dataModelExpansions.Contains(dataModelExpansion))
                    throw new ArtemisCoreException("Cannot remove a data model expansion that wasn't previously added.");

                // TODO SpoinkyNL 3-3-2018: Dispose the expansion and fire an event
                _dataModelExpansions.Remove(dataModelExpansion);
            }
        }

        public DataModelDescription GetMainDataModelDescription()
        {
            var dataModelDescription = new DataModelDescription();

            return dataModelDescription;
        }
    }
}