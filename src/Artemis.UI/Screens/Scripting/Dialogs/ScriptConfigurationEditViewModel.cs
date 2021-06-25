using System.Threading.Tasks;
using Artemis.Core.ScriptingProviders;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.Scripting.Dialogs
{
    public class ScriptConfigurationEditViewModel : DialogViewModelBase
    {
        private string _name;

        public ScriptConfigurationEditViewModel(IModelValidator<ScriptConfigurationEditViewModel> validator, ScriptConfiguration scriptConfiguration) : base(validator)
        {
            ScriptConfiguration = scriptConfiguration;
            _name = ScriptConfiguration.Name;
        }

        public ScriptConfiguration ScriptConfiguration { get; }

        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            ScriptConfiguration.Name = Name;
            if (Session is {IsEnded: false})
                Session.Close(nameof(Accept));
        }

        public void Delete()
        {
            if (Session is {IsEnded: false})
                Session.Close(nameof(Delete));
        }
    }

    public class ProfileElementScriptConfigurationEditViewModelValidator : AbstractValidator<ScriptConfigurationEditViewModel>
    {
        public ProfileElementScriptConfigurationEditViewModelValidator()
        {
            RuleFor(m => m.Name).NotEmpty().WithMessage("Script name may not be empty");
        }
    }
}