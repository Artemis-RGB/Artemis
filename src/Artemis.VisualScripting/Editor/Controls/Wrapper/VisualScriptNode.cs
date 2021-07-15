using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.Events;
using Artemis.VisualScripting.Internal;
using Artemis.VisualScripting.Model;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Editor.Controls.Wrapper
{
    public class VisualScriptNode : AbstractBindable
    {
        #region Properties & Fields

        private double _locationOffset;

        public VisualScript Script { get; }
        public INode Node { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            private set => SetProperty(ref _isSelected, value);
        }

        private ObservableCollection<VisualScriptPin> _inputPins = new();
        public ObservableCollection<VisualScriptPin> InputPins
        {
            get => _inputPins;
            private set => SetProperty(ref _inputPins, value);
        }

        private ObservableCollection<VisualScriptPin> _outputPins = new();
        public ObservableCollection<VisualScriptPin> OutputPins
        {
            get => _outputPins;
            private set => SetProperty(ref _outputPins, value);
        }

        private ObservableCollection<VisualScriptPinCollection> _inputPinCollections = new();
        public ObservableCollection<VisualScriptPinCollection> InputPinCollections
        {
            get => _inputPinCollections;
            private set => SetProperty(ref _inputPinCollections, value);
        }

        private ObservableCollection<VisualScriptPinCollection> _outputPinCollections = new();
        public ObservableCollection<VisualScriptPinCollection> OutputPinCollections
        {
            get => _outputPinCollections;
            private set => SetProperty(ref _outputPinCollections, value);
        }

        public double X
        {
            get => Node.X + _locationOffset;
            set
            {
                Node.X = value - _locationOffset;
                OnPropertyChanged();
            }
        }

        public double Y
        {
            get => Node.Y + _locationOffset;
            set
            {
                Node.Y = value - _locationOffset;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        private ActionCommand _removeCommand;
        public ActionCommand RemoveCommand => _removeCommand ??= new ActionCommand(Remove, RemoveCanExecute);

        #endregion

        #region Events

        public event EventHandler<VisualScriptNodeIsSelectedChangedEventArgs> IsSelectedChanged;
        public event EventHandler DragStarting;
        public event EventHandler DragEnding;
        public event EventHandler<VisualScriptNodeDragMovingEventArgs> DragMoving;

        #endregion

        #region Constructors

        public VisualScriptNode(VisualScript script, INode node)
        {
            this.Script = script;
            this.Node = node;

            Node.PropertyChanged += OnNodePropertyChanged;

            _locationOffset = script.SurfaceSize / 2.0;

            ValidatePins();
        }

        #endregion

        #region Methods

        private void OnNodePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (string.Equals(args.PropertyName, nameof(Node.Pins), StringComparison.OrdinalIgnoreCase)
             || string.Equals(args.PropertyName, nameof(Node.PinCollections), StringComparison.OrdinalIgnoreCase))
                ValidatePins();
        }

        private void ValidatePins()
        {
            if (Node == null)
            {
                InputPins.Clear();
                OutputPins.Clear();
                InputPinCollections.Clear();
                OutputPinCollections.Clear();
                return;
            }

            #region Remove Excessive

            HashSet<IPin> pins = new(Node.Pins);
            HashSet<IPinCollection> pinCollections = new(Node.PinCollections);

            void ValidatePinList(ObservableCollection<VisualScriptPin> list)
            {
                List<VisualScriptPin> pinsToRemove = new();
                foreach (VisualScriptPin pin in list)
                    if ((pin.Pin == null) || !pins.Contains(pin.Pin))
                        pinsToRemove.Add(pin);
                foreach (VisualScriptPin pin in pinsToRemove)
                {
                    pin.DisconnectAll();
                    list.Remove(pin);
                }
            }

            void ValidatePinCollectionList(ObservableCollection<VisualScriptPinCollection> list)
            {
                List<VisualScriptPinCollection> pinCollectionsToRemove = new();
                foreach (VisualScriptPinCollection pinCollection in list)
                    if ((pinCollection.PinCollection == null) || !pinCollections.Contains(pinCollection.PinCollection))
                        pinCollectionsToRemove.Add(pinCollection);
                foreach (VisualScriptPinCollection pinCollection in pinCollectionsToRemove)
                {
                    pinCollection.RemoveAll();
                    list.Remove(pinCollection);
                }
            }

            ValidatePinList(InputPins);
            ValidatePinList(OutputPins);
            ValidatePinCollectionList(InputPinCollections);
            ValidatePinCollectionList(OutputPinCollections);

            #endregion

            #region Add Missing

            HashSet<IPin> existingPins = new(InputPins.Concat(OutputPins).Select(x => x.Pin));
            HashSet<IPinCollection> existingPinCollections = new(InputPinCollections.Concat(OutputPinCollections).Select(x => x.PinCollection));

            foreach (IPin pin in Node.Pins)
            {
                if (pin.Direction == PinDirection.Input)
                {
                    if (!existingPins.Contains(pin))
                        InputPins.Add(new VisualScriptPin(this, pin));
                }
                else
                {
                    if (!existingPins.Contains(pin))
                        OutputPins.Add(new VisualScriptPin(this, pin));
                }
            }

            foreach (IPinCollection pinCollection in Node.PinCollections)
            {
                if (pinCollection.Direction == PinDirection.Input)
                {
                    if (!existingPinCollections.Contains(pinCollection))
                        InputPinCollections.Add(new VisualScriptPinCollection(this, pinCollection));
                }
                else
                {
                    if (!existingPinCollections.Contains(pinCollection))
                        OutputPinCollections.Add(new VisualScriptPinCollection(this, pinCollection));
                }
            }

            #endregion
        }

        public void SnapNodeToGrid()
        {
            X -= X % Script.GridSize;
            Y -= Y % Script.GridSize;
        }

        public void Select(bool alterSelection = false)
        {
            IsSelected = true;
            OnIsSelectedChanged(IsSelected, alterSelection);
        }

        public void Deselect(bool alterSelection = false)
        {
            IsSelected = false;
            OnIsSelectedChanged(IsSelected, alterSelection);
        }

        public void DragStart() => DragStarting?.Invoke(this, new EventArgs());
        public void DragEnd() => DragEnding?.Invoke(this, new EventArgs());
        public void DragMove(double dx, double dy) => DragMoving?.Invoke(this, new VisualScriptNodeDragMovingEventArgs(dx, dy));

        private void OnIsSelectedChanged(bool isSelected, bool alterSelection) => IsSelectedChanged?.Invoke(this, new VisualScriptNodeIsSelectedChangedEventArgs(isSelected, alterSelection));

        internal void OnPinConnected(PinConnectedEventArgs args) => Script.OnPinConnected(args);
        internal void OnPinDisconnected(PinDisconnectedEventArgs args) => Script.OnPinDisconnected(args);

        internal void OnIsConnectingPinChanged(VisualScriptPin isConnectingPin) => Script.OnIsConnectingPinChanged(isConnectingPin);

        private void DisconnectAllPins()
        {
            foreach (VisualScriptPin pin in InputPins)
                pin.DisconnectAll();

            foreach (VisualScriptPin pin in OutputPins)
                pin.DisconnectAll();

            foreach (VisualScriptPinCollection pinCollection in InputPinCollections)
                foreach (VisualScriptPin pin in pinCollection.Pins)
                    pin.DisconnectAll();

            foreach (VisualScriptPinCollection pinCollection in OutputPinCollections)
                foreach (VisualScriptPin pin in pinCollection.Pins)
                    pin.DisconnectAll();
        }

        private void Remove()
        {
            DisconnectAllPins();
            Script.RemoveNode(this);
        }

        private bool RemoveCanExecute() => Node is not IExitNode;

        #endregion
    }
}
