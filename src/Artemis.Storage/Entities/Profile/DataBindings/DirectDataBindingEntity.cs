using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DirectDataBindingEntity : IDataBindingModeEntity
    {
        public DirectDataBindingEntity()
        {
            Modifiers = new List<DataBindingModifierEntity>();
        }

        public Guid? SourceDataModelGuid { get; set; }
        public string SourcePropertyPath { get; set; }

        public List<DataBindingModifierEntity> Modifiers { get; set; }
    }
}