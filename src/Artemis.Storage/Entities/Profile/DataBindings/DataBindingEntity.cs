using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DataBindingEntity
    {
        public DataBindingEntity()
        {
            Modifiers = new List<DataBindingModifierEntity>();
        }

        public Guid? SourceDataModelGuid { get; set; }
        public string SourcePropertyPath { get; set; }
        public int DataBindingMode { get; set; }
        public TimeSpan EasingTime { get; set; }
        public int EasingFunction { get; set; }

        public List<DataBindingModifierEntity> Modifiers { get; set; }
    }
}