using Artemis.UI.Screens.Debugger;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Services
{
    public class DebugService : IDebugService
    {
        private readonly IWindowService _windowService;
        private DebugViewModel? _debugViewModel;

        public DebugService(IWindowService windowService)
        {
            _windowService = windowService;
        }

        public void ClearDebugger()
        {
            _debugViewModel = null;
        }

        private void BringDebuggerToForeground()
        {
            if (_debugViewModel != null)
                _debugViewModel.Activate();
        }

        private void CreateDebugger()
        {
            _debugViewModel = _windowService.ShowWindow<DebugViewModel>();
        }

        public void ShowDebugger()
        {
            if (_debugViewModel != null)
                BringDebuggerToForeground();
            else
                CreateDebugger();
        }
    }
}