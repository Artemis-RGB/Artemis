using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Artemis.Core;
using Artemis.VisualScripting.Events;
using Artemis.VisualScripting.Internal;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Editor.Controls.Wrapper
{
    public class VisualScriptPin : AbstractBindable
    {
        #region Constants

        private const double CABLE_OFFSET = 24 * 4;

        #endregion

        #region Properties & Fields

        private VisualScriptPin _isConnectingPin;
        private VisualScriptCable _isConnectingCable;

        public VisualScriptNode Node { get; }
        public IPin Pin { get; }

        public IReadOnlyCollection<VisualScriptCable> Connections => new ReadOnlyCollection<VisualScriptCable>(InternalConnections);
        internal ObservableCollection<VisualScriptCable> InternalConnections { get; } = new();

        private Point _absolutePosition;
        public Point AbsolutePosition
        {
            get => _absolutePosition;
            internal set
            {
                if (SetProperty(ref _absolutePosition, value))
                    OnPropertyChanged(nameof(AbsoluteCableTargetPosition));
            }
        }

        public Point AbsoluteCableTargetPosition => Pin.Direction == PinDirection.Input ? new Point(AbsolutePosition.X - CABLE_OFFSET, AbsolutePosition.Y)
                                                        : new Point(AbsolutePosition.X + CABLE_OFFSET, AbsolutePosition.Y);

        #endregion

        #region Constructors

        public VisualScriptPin(VisualScriptNode node, IPin pin)
        {
            this.Node = node;
            this.Pin = pin;
        }

        #endregion

        #region Methods

        public void SetConnecting(bool isConnecting)
        {
            if (isConnecting)
            {
                if (_isConnectingCable != null)
                    SetConnecting(false);

                _isConnectingPin = new VisualScriptPin(null, new IsConnectingPin(Pin.Direction == PinDirection.Input ? PinDirection.Output : PinDirection.Input, Pin.Type))
                { AbsolutePosition = AbsolutePosition };
                _isConnectingCable = new VisualScriptCable(this, _isConnectingPin);
                Node.OnIsConnectingPinChanged(_isConnectingPin);
            }
            else
            {
                _isConnectingCable.Disconnect();
                _isConnectingCable = null;
                _isConnectingPin = null;
                Node.OnIsConnectingPinChanged(_isConnectingPin);
            }
        }

        internal void ConnectTo(VisualScriptCable cable)
        {
            if (InternalConnections.Contains(cable)) return;

            if (Pin.Direction == PinDirection.Input)
            {
                List<VisualScriptCable> cables = InternalConnections.ToList();
                foreach (VisualScriptCable c in cables)
                    c.Disconnect();
            }

            InternalConnections.Add(cable);
            Pin.ConnectTo(cable.GetConnectedPin(Pin));

            Node?.OnPinConnected(new PinConnectedEventArgs(this, cable));
        }

        public void DisconnectAll()
        {
            List<VisualScriptCable> cables = InternalConnections.ToList();
            foreach (VisualScriptCable cable in cables)
                cable.Disconnect();
        }

        internal void Disconnect(VisualScriptCable cable)
        {
            InternalConnections.Remove(cable);
            Pin.DisconnectFrom(cable.GetConnectedPin(Pin));

            Node?.OnPinDisconnected(new PinDisconnectedEventArgs(this, cable));
        }

        #endregion
    }
}
