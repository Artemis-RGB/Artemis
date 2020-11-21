using System;
using System.Windows.Forms;

namespace Artemis.UI.InputProviders
{
    public sealed class SpongeWindow : NativeWindow
    {
        public SpongeWindow()
        {
            CreateHandle(new CreateParams());
        }

        public event EventHandler<Message> WndProcCalled;

        protected override void WndProc(ref Message m)
        {
            OnWndProcCalled(m);
            base.WndProc(ref m);
        }

        private void OnWndProcCalled(Message e)
        {
            WndProcCalled?.Invoke(this, e);
        }
    }
}