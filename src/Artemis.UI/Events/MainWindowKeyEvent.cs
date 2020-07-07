using System.Windows.Input;

namespace Artemis.UI.Events
{
    public class MainWindowKeyEvent
    {
        public MainWindowKeyEvent(object sender, bool keyDown, KeyEventArgs eventArgs)
        {
            Sender = sender;
            KeyDown = keyDown;
            EventArgs = eventArgs;
        }

        public object Sender { get; }
        public bool KeyDown { get; }
        public KeyEventArgs EventArgs { get; }
    }
}