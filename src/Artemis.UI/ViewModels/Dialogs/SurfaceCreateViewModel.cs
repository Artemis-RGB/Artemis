using System.Threading.Tasks;
using Stylet;

namespace Artemis.UI.ViewModels.Dialogs
{
    public class SurfaceCreateViewModel : DialogViewModelBase
    {
        public SurfaceCreateViewModel(IModelValidator<SurfaceCreateViewModel> validator) : base(validator)
        {
        }

        public string SurfaceName { get; set; }

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