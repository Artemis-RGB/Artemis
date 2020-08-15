using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Dialog;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    public class ProfileCreateViewModel : DialogViewModelBase
    {
        private string _profileName;

        public ProfileCreateViewModel(IModelValidator<ProfileCreateViewModel> validator) : base(validator)
        {
        }

        public string ProfileName
        {
            get => _profileName;
            set => SetAndNotify(ref _profileName, value);
        }

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

    public class ProfileCreateViewModelValidator : AbstractValidator<ProfileCreateViewModel>
    {
        public ProfileCreateViewModelValidator()
        {
            RuleFor(m => m.ProfileName).NotEmpty().WithMessage("Profile name may not be empty");
        }
    }
}