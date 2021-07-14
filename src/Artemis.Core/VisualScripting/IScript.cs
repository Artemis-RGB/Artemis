using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Artemis.Core.VisualScripting
{
    public interface IScript : INotifyPropertyChanged, IDisposable
    {
        string Name { get; }
        string Description { get; }

        IEnumerable<INode> Nodes { get; }

        Type ResultType { get; }

        void Run();
        void AddNode(INode node);
        void RemoveNode(INode node);
    }

    public interface IScript<out T> : IScript
    {
        T Result { get; }
    }
}
