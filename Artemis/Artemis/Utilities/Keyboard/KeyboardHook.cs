using System.Threading.Tasks;
using System.Windows.Forms;
using VirtualInput;

namespace Artemis.Utilities.Keyboard
{
    public class KeyboardHook
    {
        public delegate void KeyDownCallbackHandler(KeyEventArgs e);

        public KeyboardHook()
        {
            VirtualKeyboard.KeyDown += VirtualKeyboardOnKeyDown;
            VirtualKeyboard.StartInterceptor();
        }

        private void VirtualKeyboardOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            Task.Factory.StartNew(() => { KeyDownCallback?.Invoke(keyEventArgs); });
        }
        
        public event KeyDownCallbackHandler KeyDownCallback;
    }
}