using System;
using System.Collections.ObjectModel;
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

            _locationOffset = script.SurfaceSize / 2.0;

            foreach (IPin pin in node.Pins)
            {
                if (pin.Direction == PinDirection.Input)
                    InputPins.Add(new VisualScriptPin(this, pin));
                else
                    OutputPins.Add(new VisualScriptPin(this, pin));
            }

            foreach (IPinCollection pinCollection in node.PinCollections)
            {
                if (pinCollection.Direction == PinDirection.Input)
                    InputPinCollections.Add(new VisualScriptPinCollection(this, pinCollection));
                else
                    OutputPinCollections.Add(new VisualScriptPinCollection(this, pinCollection));
            }
        }

        #endregion

        #region Methods

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
