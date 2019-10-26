using System.Threading.Tasks;
using Artemis.UI.ViewModels.Dialogs;
using Ninject.Parameters;

namespace Artemis.UI.Services.Interfaces
{
    public interface IDialogService : IArtemisUIService
    {
        Task<bool> ShowConfirmDialog(string header, string text, string confirmText = "Confirm", string cancelText = "Cancel");
        Task<bool> ShowConfirmDialogAt(string identifier, string header, string text, string confirmText = "Confirm", string cancelText = "Cancel");
        Task<object> ShowDialog<T>(IParameter[] parameters = null) where T : DialogViewModelBase;
        Task<object> ShowDialogAt<T>(string identifier, IParameter[] parameters = null) where T : DialogViewModelBase;
    }
}