using System.Threading.Tasks;
using Artemis.UI.ViewModels.Dialogs;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.Dialogs
{
    public class ProfileCreateViewModel : DialogViewModelBase
    {
        public ProfileCreateViewModel(IModelValidator<ProfileCreateViewModel> validator) : base(validator)
        {
        }

        public string ProfileName { get; set; }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            Session.Close(ProfileName);
        }

        public void Cancel()
        {
            Session.Close();
        }
    }
}