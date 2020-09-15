using System;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DataBindingEntity
    {
        public string TargetExpression { get; set; }
        public TimeSpan EasingTime { get; set; }
        public int EasingFunction { get; set; }

        public IDataBindingModeEntity DataBindingMode { get; set; }
    }
}