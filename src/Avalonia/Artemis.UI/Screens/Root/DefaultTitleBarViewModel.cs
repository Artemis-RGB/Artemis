using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Root
{
    public class DefaultTitleBarViewModel : ViewModelBase
    {
        private readonly IDebugService _debugService;

        public DefaultTitleBarViewModel(IDebugService debugService)
        {
            _debugService = debugService;
        }

        public void ShowDebugger()
        {
            _debugService.ShowDebugger();
        }
    }
}