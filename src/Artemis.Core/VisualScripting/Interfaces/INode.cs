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
        public object? Storage { get; set; }

        public IReadOnlyCollection<IPin> Pins { get; }
        public IReadOnlyCollection<IPinCollection> PinCollections { get; }

        event EventHandler Resetting;

        void Initialize(INodeScript script);
        void Evaluate();
        void Reset();
    }
}