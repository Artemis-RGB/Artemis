using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DirectDataBindingEntity : IDataBindingModeEntity
    {
        public DirectDataBindingEntity()
        {
            Modifiers = new List<DataBindingModifierEntity>();
        }

        public DataModelPathEntity SourcePath { get; set; }
        public List<DataBindingModifierEntity> Modifiers { get; set; }
    }
}