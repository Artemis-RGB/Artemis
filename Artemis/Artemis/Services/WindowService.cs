using System.Dynamic;
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

        public T ShowWindow<T>(string windowName, dynamic settings = null, params IParameter[] param) where T : class
        {
            var windowManager = new WindowManager();
            var viewModel = _kernel.Get<T>(param);

            if (settings == null)
                settings = new ExpandoObject();
            settings.Title = windowName;
            windowManager.ShowWindow(viewModel, null, settings);
            return viewModel;
        }

        public T ShowDialog<T>(string dialogName, dynamic settings = null, params IParameter[] param) where T : class
        {
            var windowManager = new WindowManager();
            var viewModel = _kernel.Get<T>(param);

            if (settings == null)
                settings = new ExpandoObject();
            settings.Title = dialogName;
            windowManager.ShowDialog(viewModel, null, settings);
            return viewModel;
        }
    }
}