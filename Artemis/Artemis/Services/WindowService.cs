using Caliburn.Micro;
using Ninject;
using Ninject.Parameters;

namespace Artemis.Services
{
    public class WindowService
    {
        private readonly IKernel _kernel;

        public WindowService(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void ShowWindow<T>(params IParameter[] param) where T : class
        {
            var windowManager = new WindowManager();
            var viewModel = _kernel.Get<T>(param);

            windowManager.ShowWindow(viewModel);
        }

        public void ShowDialog<T>(params IParameter[] param) where T : class
        {
            var windowManager = new WindowManager();
            var viewModel = _kernel.Get<T>(param);

            windowManager.ShowDialog(viewModel);
        }
    }
}