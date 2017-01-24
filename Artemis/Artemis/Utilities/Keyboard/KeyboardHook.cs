using System.Threading.Tasks;
using System.Windows.Forms;
using Open.WinKeyboardHook;

namespace Artemis.Utilities.Keyboard
{
    public static class KeyboardHook
    {
        private static KeyboardInterceptor _interceptor;

        public delegate void KeyDownCallbackHandler(KeyEventArgs e);

        public static void SetupKeyboardHook()
        {
            _interceptor = new KeyboardInterceptor();
            _interceptor.KeyDown += VirtualKeyboardOnKeyDown;
            _interceptor.StartCapturing();
        }

        private static async void VirtualKeyboardOnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            await Task.Factory.StartNew(() => { KeyDownCallback?.Invoke(keyEventArgs); });
        }

        public static event KeyDownCallbackHandler KeyDownCallback;
    }
}