using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.Events;

namespace Artemis.Core
{
    public abstract class Pin : CorePropertyChanged, IPin
    {
        #region Properties & Fields

        public INode Node { get; }

        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        private bool _isEvaluated;

        public bool IsEvaluated
        {
            get => _isEvaluated;
            set => SetAndNotify(ref _isEvaluated, value);
        }

        private readonly List<IPin> _connectedTo = new();
        private string _name;
        public IReadOnlyList<IPin> ConnectedTo => new ReadOnlyCollection<IPin>(_connectedTo);

        public abstract PinDirection Direction { get; }
        public abstract Type Type { get; }
        public abstract object PinValue { get; }

        #endregion

        #region Events

        public event EventHandler<SingleValueEventArgs<IPin>> PinConnected;
        public event EventHandler<SingleValueEventArgs<IPin>> PinDisconnected;

        #endregion

        #region Constructors

        protected Pin(INode node, string name = "")
        {
            this.Node = node;
            this.Name = name;

            if (Node != null)
                Node.Resetting += OnNodeResetting;
        }

        #endregion

        #region Methods

        public void ConnectTo(IPin pin)
        {
            _connectedTo.Add(pin);
            OnPropertyChanged(nameof(ConnectedTo));

            PinConnected?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        }

        public void DisconnectFrom(IPin pin)
        {
            _connectedTo.Remove(pin);
            OnPropertyChanged(nameof(ConnectedTo));

            PinDisconnected?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        }

        public void DisconnectAll()
        {
            List<IPin> connectedPins = new(_connectedTo);

            _connectedTo.Clear();
            OnPropertyChanged(nameof(ConnectedTo));

            foreach (IPin pin in connectedPins)
                PinDisconnected?.Invoke(this, new SingleValueEventArgs<IPin>(pin));
        }

        private void OnNodeResetting(object sender, EventArgs e)
        {
            IsEvaluated = false;
        }

        #endregion
    }
}