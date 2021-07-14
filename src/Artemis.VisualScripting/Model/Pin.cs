using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Model
{
    public abstract class Pin : AbstractBindable, IPin
    {
        #region Properties & Fields

        public INode Node { get; }
        public string Name { get; }

        private bool _isEvaluated;
        public bool IsEvaluated
        {
            get => _isEvaluated;
            set => SetProperty(ref _isEvaluated, value);
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
        }

        public void DisconnectFrom(IPin pin)
        {
            _connectedTo.Remove(pin);
        }

        private void OnNodeResetting(object sender, EventArgs e)
        {
            IsEvaluated = false;
        }

        #endregion
    }
}
