using Artemis.UI.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.ViewModels.Utilities
{
    public class DialogViewModelHost : PropertyChangedBase
    {
        private readonly IViewManager _viewManager;

        public DialogViewModelHost(IViewManager viewManager)
        {
            _viewManager = viewManager;
        }

        public DialogViewModelBase ActiveDialogViewModel { get; set; }
        public bool IsOpen { get; set; }

        public void OpenDialog(DialogViewModelBase viewModel, string dialogIdentifier)
        {
            var view = _viewManager.CreateViewForModel(viewModel);
            DialogHost.Show(view, dialogIdentifier, viewModel.OnDialogClosed);
            viewModel.DialogViewModelHost = this;

            ActiveDialogViewModel = viewModel;
            IsOpen = true;
        }
    }
}