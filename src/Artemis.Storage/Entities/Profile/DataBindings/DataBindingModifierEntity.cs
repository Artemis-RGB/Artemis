using System;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DataBindingModifierEntity
    {
        public string ModifierType { get; set; }
        public Guid? ModifierTypePluginGuid { get; set; }

        public int Order { get; set; }
        public int ParameterType { get; set; }

        public DataModelPathEntity ParameterPath { get; set; }

        // Stored as a string to be able to control serialization and deserialization ourselves
        public string ParameterStaticValue { get; set; }
    }
}