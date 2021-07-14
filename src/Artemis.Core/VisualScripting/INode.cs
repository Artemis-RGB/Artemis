using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Artemis.Core.VisualScripting
{
    public interface INode : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }

        public double X { get; set; }
        public double Y { get; set; }

        public IReadOnlyCollection<IPin> Pins { get; }
        public IReadOnlyCollection<IPinCollection> PinCollections { get; }

        public object CustomView { get; }
        public object CustomViewModel { get; }

        event EventHandler Resetting;

        void Evaluate();
        void Reset();
    }
}
