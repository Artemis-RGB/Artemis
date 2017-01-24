using System.Threading.Tasks;
using System.Windows.Forms;
using Open.WinKeyboardHook;

namespace Artemis.Utilities.Keyboard
{
    public static class KeyboardHook
    {
        public delegate void KeyDownCallbackHandler(KeyEventArgs e);

        static KeyboardHook()
        {
            var interceptor = new KeyboardInterceptor();
            interceptor.KeyDown += VirtualKeyboardOnKeyDown;
            interceptor.StartCapturing();
        }

        private static async void VirtualKeyboardOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            await Task.Factory.StartNew(() => { KeyDownCallback?.Invoke(keyEventArgs); });
        }

        public static event KeyDownCallbackHandler KeyDownCallback;
    }
}