using Ninject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ninject.Parameters;

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

        private object? _storage;
        public object? Storage
        {
            get => _storage;
            set => SetAndNotify(ref _storage, value);
        }

        public virtual bool IsExitNode => false;
        public virtual bool IsDefaultNode => false;

        private readonly List<IPin> _pins = new();
        public IReadOnlyCollection<IPin> Pins => new ReadOnlyCollection<IPin>(_pins);

        private readonly List<IPinCollection> _pinCollections = new();
        public IReadOnlyCollection<IPinCollection> PinCollections => new ReadOnlyCollection<IPinCollection>(_pinCollections);

        #endregion

        #region Events

        public event EventHandler Resetting;

        #endregion

        #region Construtors

        protected Node()
        { }

        protected Node(string name, string description)
        {
            Name = name;
            Description = description;
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

        public virtual void Initialize(INodeScript script)
        { }

        public abstract void Evaluate();

        public virtual void Reset()
        {
            Resetting?.Invoke(this, new EventArgs());
        }

        #endregion
    }

    public abstract class Node<T> : CustomViewModelNode where T : ICustomNodeViewModel
    {
        [Inject]
        internal IKernel Kernel { get; set; } = null!;

        protected Node()
        { }

        protected Node(string name, string description) : base(name, description)
        { }

        public virtual T GetViewModel()
        {
            return Kernel.Get<T>(new ConstructorArgument("node", this));
        }

        public override ICustomNodeViewModel GetCustomViewModel()
        {
            return GetViewModel();
        }
    }

    public abstract class CustomViewModelNode : Node
    {
        /// <inheritdoc />
        protected CustomViewModelNode()
        { }

        /// <inheritdoc />
        protected CustomViewModelNode(string name, string description) : base(name, description)
        { }

        public abstract ICustomNodeViewModel GetCustomViewModel();
    }
}