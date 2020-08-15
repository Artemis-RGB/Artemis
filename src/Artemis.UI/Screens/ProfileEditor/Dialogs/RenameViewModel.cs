using System.Threading.Tasks;
using Artemis.UI.Shared.Services.Dialog;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Dialogs
{
    public class RenameViewModel : DialogViewModelBase
    {
        private string _elementName;

        public RenameViewModel(IModelValidator<RenameViewModel> validator, string subject, string currentName) : base(validator)
        {
            Subject = subject;
            ElementName = currentName;
        }

        public string Subject { get; }

        public string ElementName
        {
            get => _elementName;
            set => SetAndNotify(ref _elementName, value);
        }

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

    public class ProfileElementRenameViewModelValidator : AbstractValidator<RenameViewModel>
    {
        public ProfileElementRenameViewModelValidator()
        {
            RuleFor(m => m.ElementName).NotEmpty().WithMessage("Element name may not be empty");
        }
    }
}