using Artemis.UI.Avalonia.Screens.Debugger;
using Artemis.UI.Avalonia.Services.Interfaces;
using Artemis.UI.Avalonia.Shared.Services.Interfaces;

namespace Artemis.UI.Avalonia.Services
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
                _debugViewModel.IsActive = true;
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