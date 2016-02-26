using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Artemis.Utilities.Keyboard
{
    public class KeyboardHook
    {
        private IKeyboardMouseEvents _mGlobalHook;
        public int Subscriptions { get; set; }

        public void Subscribe(KeyEventHandler handleKeypress)
        {
            if (_mGlobalHook == null)
                _mGlobalHook = Hook.GlobalEvents();

            _mGlobalHook.KeyDown += handleKeypress;
            Subscriptions++;
        }

        public void Unsubscribe(KeyEventHandler handleKeypress)
        {
            if (_mGlobalHook == null)
                return;

            _mGlobalHook.KeyDown -= handleKeypress;
            Subscriptions--;

            if (Subscriptions >= 1)
                return;
            _mGlobalHook.Dispose();
            _mGlobalHook = null;
        }
    }
}