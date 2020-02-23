using System.Threading.Tasks;
using Artemis.Core.Models.Profile;
using Artemis.UI.Shared.Services.Dialog;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.Dialogs
{
    public class ProfileElementRenameViewModel : DialogViewModelBase
    {
        public ProfileElementRenameViewModel(IModelValidator<ProfileElementRenameViewModel> validator, ProfileElement profileElement) : base(validator)
        {
            ElementName = profileElement.Name;
        }

        public string ElementName { get; set; }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            Session.Close(ElementName);
        }

        public void Cancel()
        {
            Session.Close();
        }
    }

    public class ProfileElementRenameViewModelValidator : AbstractValidator<ProfileElementRenameViewModel>
    {
        public ProfileElementRenameViewModelValidator()
        {
            RuleFor(m => m.ElementName).NotEmpty().WithMessage("Element name may not be empty");
        }
    }
}