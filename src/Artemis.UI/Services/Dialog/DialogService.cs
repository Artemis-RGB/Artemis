using System.Threading.Tasks;
using Artemis.UI.Screens.Dialogs;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;
using Ninject;
using Ninject.Parameters;
using Stylet;

namespace Artemis.UI.Services
{
    public class DialogService : IDialogService
    {
        private readonly IKernel _kernel;
        private readonly IViewManager _viewManager;

        public DialogService(IKernel kernel, IViewManager viewManager)
        {
            _kernel = kernel;
            _viewManager = viewManager;
        }

        public async Task<bool> ShowConfirmDialog(string header, string text, string confirmText = "Confirm", string cancelText = "Cancel")
        {
            var arguments = new IParameter[]
            {
                new ConstructorArgument("header", header),
                new ConstructorArgument("text", text),
                new ConstructorArgument("confirmText", confirmText),
                new ConstructorArgument("cancelText", cancelText)
            };
            var result = await ShowDialog<ConfirmDialogViewModel>(arguments);
            return (bool) result;
        }

        public async Task<bool> ShowConfirmDialogAt(string identifier, string header, string text, string confirmText = "Confirm", string cancelText = "Cancel")
        {
            var arguments = new IParameter[]
            {
                new ConstructorArgument("header", header),
                new ConstructorArgument("text", text),
                new ConstructorArgument("confirmText", confirmText),
                new ConstructorArgument("cancelText", cancelText)
            };
            var result = await ShowDialogAt<ConfirmDialogViewModel>(identifier, arguments);
            return (bool) result;
        }

        public async Task<object> ShowDialog<T>(IParameter[] parameters = null) where T : DialogViewModelBase
        {
            var viewModel = parameters != null ? _kernel.Get<T>(parameters) : _kernel.Get<T>();
            return await ShowDialog(null, viewModel);
        }

        public async Task<object> ShowDialogAt<T>(string identifier, IParameter[] parameters = null) where T : DialogViewModelBase
        {
            var viewModel = parameters != null ? _kernel.Get<T>(parameters) : _kernel.Get<T>();
            return await ShowDialog(identifier, viewModel);
        }

        private async Task<object> ShowDialog(string identifier, DialogViewModelBase viewModel)
        {
            var view = _viewManager.CreateViewForModel(viewModel);
            _viewManager.BindViewToModel(view, viewModel);

            if (identifier == null)
                return await DialogHost.Show(view, viewModel.OnDialogOpened, viewModel.OnDialogClosed);
            return await DialogHost.Show(view, identifier, viewModel.OnDialogOpened, viewModel.OnDialogClosed);
        }
    }
}