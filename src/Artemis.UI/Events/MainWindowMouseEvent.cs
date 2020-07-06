using System.Windows.Input;

namespace Artemis.UI.Events
{
    public class MainWindowMouseEvent
    {
        public MainWindowMouseEvent(object sender, bool keyDown, MouseEventArgs eventArgs)
        {
            Sender = sender;
            MouseDown = keyDown;
            EventArgs = eventArgs;
        }

        public object Sender { get; }
        public bool MouseDown { get; }
        public MouseEventArgs EventArgs { get; }
    }
}