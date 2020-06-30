using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Dialog;
using Stylet;

namespace Artemis.UI.Screens.SurfaceEditor.Dialogs
{
    public class SurfaceCreateViewModel : DialogViewModelBase
    {
        private string _surfaceName;

        public SurfaceCreateViewModel(IModelValidator<SurfaceCreateViewModel> validator) : base(validator)
        {
        }

        public string SurfaceName
        {
            get => _surfaceName;
            set => SetAndNotify(ref _surfaceName, value);
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            Session.Close(SurfaceName);
        }

        public void Cancel()
        {
            Session.Close();
        }
    }
}