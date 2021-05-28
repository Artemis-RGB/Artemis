using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.Sidebar.Dialogs
{
    public class ProfileEditViewModel : DialogViewModelBase
    {
        private string _profileName;

        public ProfileEditViewModel(IModelValidator<ProfileEditViewModel> validator, Profile profile) : base(validator)
        {
            ProfileName = profile.Name;
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
    }

    public class ProfileEditViewModelValidator : AbstractValidator<ProfileEditViewModel>
    {
        public ProfileEditViewModelValidator()
        {
            RuleFor(m => m.ProfileName).NotEmpty().WithMessage("Profile name may not be empty");
        }
    }
}