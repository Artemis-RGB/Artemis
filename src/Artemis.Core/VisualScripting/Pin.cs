using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Artemis.Core
{
    public abstract class Pin : CorePropertyChanged, IPin
    {
        #region Properties & Fields

        public INode Node { get; }
        public string Name { get; }

        private bool _isEvaluated;
        public bool IsEvaluated
        {
            get => _isEvaluated;
            set => SetAndNotify(ref _isEvaluated, value);
        }

        private readonly List<IPin> _connectedTo = new();
        public IReadOnlyList<IPin> ConnectedTo => new ReadOnlyCollection<IPin>(_connectedTo);

        public abstract PinDirection Direction { get; }
        public abstract Type Type { get; }
        public abstract object PinValue { get; }

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
        }

        public void DisconnectFrom(IPin pin)
        {
            _connectedTo.Remove(pin);
            OnPropertyChanged(nameof(ConnectedTo));
        }

        public void DisconnectAll()
        {
            _connectedTo.Clear();
            OnPropertyChanged(nameof(ConnectedTo));
        }

        private void OnNodeResetting(object sender, EventArgs e)
        {
            IsEvaluated = false;
        }

        #endregion
    }
}
