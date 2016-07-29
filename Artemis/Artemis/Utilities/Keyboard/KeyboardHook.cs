using System.Threading.Tasks;
using System.Windows.Forms;
using VirtualInput;

namespace Artemis.Utilities.Keyboard
{
    public static class KeyboardHook
    {
        public delegate void KeyDownCallbackHandler(KeyEventArgs e);

        static KeyboardHook()
        {
            VirtualKeyboard.StartInterceptor();
            VirtualKeyboard.KeyDown += VirtualKeyboardOnKeyDown;
        }

        private static void VirtualKeyboardOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            Task.Factory.StartNew(() => { KeyDownCallback?.Invoke(keyEventArgs); });
        }

        public static event KeyDownCallbackHandler KeyDownCallback;
    }
}