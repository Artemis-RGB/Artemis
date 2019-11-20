using FluentValidation;

namespace Artemis.UI.Screens.Module.ProfileEditor.Dialogs
{
    public class ProfileCreateViewModelValidator : AbstractValidator<ProfileCreateViewModel>
    {
        public ProfileCreateViewModelValidator()
        {
            RuleFor(m => m.ProfileName).NotEmpty().WithMessage("Profile name may not be empty");
        }
    }
}