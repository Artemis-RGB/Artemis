using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Artemis.Core
{
    public interface INode : INotifyPropertyChanged
    {
        string Name { get; }
        string Description { get; }
        bool IsExitNode { get; }

        public double X { get; set; }
        public double Y { get; set; }

        public IReadOnlyCollection<IPin> Pins { get; }
        public IReadOnlyCollection<IPinCollection> PinCollections { get; }

        public Type? CustomViewModelType { get; }
        public object? CustomViewModel { get; set; }

        event EventHandler Resetting;

        void Evaluate();
        void Reset();
    }
}
