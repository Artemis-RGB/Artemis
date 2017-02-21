using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Artemis.Utilities.Keyboard
{
    public static class KeyboardHook
    {
        public delegate void KeyCallbackHandler(KeyEventArgs e);

        public delegate void MouseCallbackHandler(MouseEventArgs e);

        private static IKeyboardMouseEvents _globalHook;

        public static void SetupKeyboardHook()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHookOnKeyDown;
            _globalHook.KeyUp += GlobalHookOnKeyUp;
            _globalHook.MouseDown += GlobalHookOnMouseDown;
            _globalHook.MouseUp += GlobalHookOnMouseUp;
        }

        private static async void GlobalHookOnMouseDown(object sender, MouseEventArgs e)
        {
            await Task.Factory.StartNew(() => { MouseDownCallback?.Invoke(e); });
        }

        private static async void GlobalHookOnMouseUp(object sender, MouseEventArgs e)
        {
            await Task.Factory.StartNew(() => { MouseUpCallback?.Invoke(e); });
        }

        private static async void GlobalHookOnKeyDown(object sender, KeyEventArgs e)
        {
            await Task.Factory.StartNew(() => { KeyDownCallback?.Invoke(e); });
        }

        private static async void GlobalHookOnKeyUp(object sender, KeyEventArgs e)
        {
            await Task.Factory.StartNew(() => { KeyUpCallback?.Invoke(e); });
        }

        public static event KeyCallbackHandler KeyDownCallback;
        public static event KeyCallbackHandler KeyUpCallback;
        public static event MouseCallbackHandler MouseDownCallback;
        public static event MouseCallbackHandler MouseUpCallback;
    }
}
