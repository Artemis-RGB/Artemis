using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.Events;
using Artemis.VisualScripting.Events;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Editor.Controls.Wrapper
{
    public class VisualScriptPinCollection : AbstractBindable
    {
        #region Properties & Fields

        public VisualScriptNode Node { get; }
        public IPinCollection PinCollection { get; }

        private readonly Dictionary<IPin, VisualScriptPin> _pinMapping = new();
        public ObservableCollection<VisualScriptPin> Pins { get; } = new();

        #endregion

        #region Commands

        private ActionCommand _addPinCommand;
        public ActionCommand AddPinCommand => _addPinCommand ??= new ActionCommand(AddPin);

        private ActionCommand<VisualScriptPin> _removePinCommand;
        public ActionCommand<VisualScriptPin> RemovePinCommand => _removePinCommand ??= new ActionCommand<VisualScriptPin>(RemovePin);

        #endregion

        #region Constructors

        public VisualScriptPinCollection(VisualScriptNode node, IPinCollection pinCollection)
        {
            this.Node = node;
            this.PinCollection = pinCollection;

            PinAddedEventManager.AddHandler(pinCollection, OnPinCollectionPinAdded);
            PinRemovedEventManager.AddHandler(pinCollection, OnPinCollectionPinRemoved);

            foreach (IPin pin in PinCollection)
            {
                VisualScriptPin visualScriptPin = new(node, pin);
                _pinMapping.Add(pin, visualScriptPin);
                Pins.Add(visualScriptPin);
            }
        }

        #endregion

        #region Methods

        private void OnPinCollectionPinRemoved(object sender, SingleValueEventArgs<IPin> args)
        {
            if (!_pinMapping.TryGetValue(args.Value, out VisualScriptPin visualScriptPin)) return;

            visualScriptPin.DisconnectAll();
            Pins.Remove(visualScriptPin);
        }

        private void OnPinCollectionPinAdded(object sender, SingleValueEventArgs<IPin> args)
        {
            if (_pinMapping.ContainsKey(args.Value)) return;

            VisualScriptPin visualScriptPin = new(Node, args.Value);
            _pinMapping.Add(args.Value, visualScriptPin);
            Pins.Add(visualScriptPin);
        }

        public void AddPin() => PinCollection.AddPin();

        public void RemovePin(VisualScriptPin pin) => PinCollection.Remove(pin.Pin);

        public void RemoveAll()
        {
            List<VisualScriptPin> pins = new(Pins);
            foreach (VisualScriptPin pin in pins)
                RemovePin(pin);
        }

        #endregion
    }
}
