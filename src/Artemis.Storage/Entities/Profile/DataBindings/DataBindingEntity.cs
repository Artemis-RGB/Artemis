using System;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.DataBindings
{
    public class DataBindingEntity
    {
        public string Identifier { get; set; }
        public TimeSpan EasingTime { get; set; }
        public int EasingFunction { get; set; }
        
        public NodeScriptEntity NodeScript { get; set; }
    }
}