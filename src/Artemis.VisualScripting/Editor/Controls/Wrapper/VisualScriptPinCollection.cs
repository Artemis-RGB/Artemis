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

            foreach (IPin pin in PinCollection)
                Pins.Add(new VisualScriptPin(node, pin));
        }

        #endregion

        #region Methods

        public void AddPin()
        {
            IPin pin = PinCollection.AddPin();
            Pins.Add(new VisualScriptPin(Node, pin));
        }

        public void RemovePin(VisualScriptPin pin)
        {
            pin.DisconnectAll();
            PinCollection.Remove(pin.Pin);
            Pins.Remove(pin);
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
