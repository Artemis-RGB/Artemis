using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Model
{
    public abstract class Node : AbstractBindable, INode
    {
        #region Properties & Fields

        private string _name;
        public string Name
        {
            get => _name;
            protected set => SetProperty(ref _name, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            protected set => SetProperty(ref _description, value);
        }

        private double _x;
        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }

        private double _y;
        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }

        private readonly List<IPin> _pins = new();
        public IReadOnlyCollection<IPin> Pins => new ReadOnlyCollection<IPin>(_pins);

        private readonly List<IPinCollection> _pinCollections = new();
        public IReadOnlyCollection<IPinCollection> PinCollections => new ReadOnlyCollection<IPinCollection>(_pinCollections);

        public object CustomView { get; private set; }
        public object CustomViewModel { get; private set; }

        #endregion

        #region Events

        public event EventHandler Resetting;

        #endregion

        #region Construtors

        protected Node()
        { }

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

        protected void RegisterCustomView(DataTemplate view, object viewModel)
        {
            CustomView = view;
            CustomViewModel = viewModel;
        }

        public abstract void Evaluate();

        public virtual void Reset()
        {
            Resetting?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
