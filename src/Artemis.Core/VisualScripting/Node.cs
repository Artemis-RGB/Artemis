using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Artemis.Core
{
    public abstract class Node : CorePropertyChanged, INode
    {
        #region Properties & Fields

        private string _name;

        public string Name
        {
            get => _name;
            protected set => SetAndNotify(ref _name, value);
        }

        private string _description;

        public string Description
        {
            get => _description;
            protected set => SetAndNotify(ref _description, value);
        }

        private double _x;

        public double X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        private double _y;

        public double Y
        {
            get => _y;
            set => SetAndNotify(ref _y, value);
        }

        public virtual bool IsExitNode => false;

        private readonly List<IPin> _pins = new();
        public IReadOnlyCollection<IPin> Pins => new ReadOnlyCollection<IPin>(_pins);

        private readonly List<IPinCollection> _pinCollections = new();
        public IReadOnlyCollection<IPinCollection> PinCollections => new ReadOnlyCollection<IPinCollection>(_pinCollections);

        public Type? CustomViewModelType { get; private set; }
        public object? CustomViewModel { get; set; }

        #endregion

        #region Events

        public event EventHandler Resetting;

        #endregion

        #region Construtors

        protected Node()
        {
        }

        protected Node(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        #endregion

        #region Methods

        protected InputPin<T> CreateInputPin<T>(string name = "")
        {
            InputPin<T> pin = new(this, name);
            _pins.Add(pin);
            OnPropertyChanged(nameof(Pins));
            return pin;
        }

        protected InputPin CreateInputPin(Type type, string name = "")
        {
            InputPin pin = new(this, type, name);
            _pins.Add(pin);
            OnPropertyChanged(nameof(Pins));
            return pin;
        }

        protected OutputPin<T> CreateOutputPin<T>(string name = "")
        {
            OutputPin<T> pin = new(this, name);
            _pins.Add(pin);
            OnPropertyChanged(nameof(Pins));
            return pin;
        }

        protected OutputPin CreateOutputPin(Type type, string name = "")
        {
            OutputPin pin = new(this, type, name);
            _pins.Add(pin);
            OnPropertyChanged(nameof(Pins));
            return pin;
        }

        protected bool RemovePin(Pin pin)
        {
            bool isRemoved = _pins.Remove(pin);
            if (isRemoved)
            {
                pin.DisconnectAll();
                OnPropertyChanged(nameof(Pins));
            }

            return isRemoved;
        }

        protected InputPinCollection<T> CreateInputPinCollection<T>(string name = "", int initialCount = 1)
        {
            InputPinCollection<T> pin = new(this, name, initialCount);
            _pinCollections.Add(pin);
            OnPropertyChanged(nameof(PinCollections));
            return pin;
        }

        protected OutputPinCollection<T> CreateOutputPinCollection<T>(string name = "", int initialCount = 1)
        {
            OutputPinCollection<T> pin = new(this, name, initialCount);
            _pinCollections.Add(pin);
            OnPropertyChanged(nameof(PinCollections));
            return pin;
        }

        protected void RegisterCustomViewModel<T>()
        {
            CustomViewModelType = typeof(T);
        }

        public abstract void Evaluate();

        public virtual void Reset()
        {
            Resetting?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}