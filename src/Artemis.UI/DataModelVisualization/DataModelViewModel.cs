using System;
using System.Linq;
using Artemis.Core.Attributes;
using Artemis.Core.Plugins.Abstract.DataModels;
using Artemis.UI.Exceptions;
using Stylet;

namespace Artemis.UI.DataModelVisualization
{
    public class DataModelViewModel : PropertyChangedBase
    {
        public DataModelViewModel(DataModel dataModel)
        {
            if (!DataModel.Initialized)
                throw new ArtemisUIException("Cannot create view model for data model that is not yet initialized");

            DataModel = dataModel;
        }

        public DataModelViewModel(DataModel parent, DataModel dataModel)
        {
            if (!DataModel.Initialized)
                throw new ArtemisUIException("Cannot create view model for data model that is not yet initialized");

            Parent = parent;
            DataModel = dataModel;
        }

        public DataModel DataModel { get; }
        public DataModel Parent { get; set; }


        private void PopulateProperties()
        {
            foreach (var propertyInfo in DataModel.GetType().GetProperties())
            {
                var dataModelPropertyAttribute = (DataModelPropertyAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(DataModelPropertyAttribute));
                if (dataModelPropertyAttribute == null)
                    continue;

                // For child data models create another data model view model
                if (typeof(DataModel).IsAssignableFrom(propertyInfo.PropertyType))
                {
                }
                // For primitives, create a property view model
                else if (propertyInfo.PropertyType.IsPrimitive)
                {
                }
                // For anything else check if it has any child primitives and if so create a property container view model
                else if (propertyInfo.PropertyType.GetProperties().Any(p => p.PropertyType.IsPrimitive))
                {
                }
            }
        }
    }
}