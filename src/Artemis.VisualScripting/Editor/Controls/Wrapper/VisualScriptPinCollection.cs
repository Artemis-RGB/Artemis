using System.Collections.Generic;
using System.Collections.ObjectModel;
using Artemis.Core;
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

            pinCollection.PinAdded += OnPinCollectionPinAdded;
            pinCollection.PinRemoved += OnPinCollectionPinRemoved;

            foreach (IPin pin in PinCollection)
            {
                VisualScriptPin visualScriptPin = new(node, pin);
                _pinMapping.Add(pin, visualScriptPin);
                Pins.Add(visualScriptPin);
            }
        }

        #endregion

        #region Methods

        private void OnPinCollectionPinRemoved(object sender, IPin pin)
        {
            if (!_pinMapping.TryGetValue(pin, out VisualScriptPin visualScriptPin)) return;

            visualScriptPin.DisconnectAll();
            Pins.Remove(visualScriptPin);
        }

        private void OnPinCollectionPinAdded(object sender, IPin pin)
        {
            if (_pinMapping.ContainsKey(pin)) return;

            VisualScriptPin visualScriptPin = new(Node, pin);
            _pinMapping.Add(pin, visualScriptPin);
            Pins.Add(visualScriptPin);
        }

        public void AddPin()
        {
            PinCollection.AddPin();
        }

        public void RemovePin(VisualScriptPin pin)
        {
            PinCollection.Remove(pin.Pin);
        }
        public void RemoveAll()
        {
            List<VisualScriptPin> pins = new(Pins);
            foreach (VisualScriptPin pin in pins)
                RemovePin(pin);
        }

        #endregion
    }
}
