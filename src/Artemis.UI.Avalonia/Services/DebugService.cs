using Artemis.UI.Avalonia.Screens.Debug;
using Artemis.UI.Avalonia.Services.Interfaces;
using Ninject;

namespace Artemis.UI.Avalonia.Services
{
    public class DebugService : IDebugService
    {
        private readonly IKernel _kernel;
        private DebugWindow? _debugWindow;

        public DebugService(IKernel kernel)
        {
            _kernel = kernel;
        }

        private void CreateDebugger()
        {
        }

        private void BringDebuggerToForeground()
        {
        }

        public void ShowDebugger()
        {
            if (_debugWindow != null)
                BringDebuggerToForeground();
            else
                CreateDebugger();
        }
    }
}