using System.Threading.Tasks;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.Scripting.Dialogs
{
    public class ScriptConfigurationCreateViewModel : DialogViewModelBase
    {
        private string _name;
        private ScriptingProvider _selectedScriptingProvider;

        public ScriptConfigurationCreateViewModel(IModelValidator<ScriptConfigurationCreateViewModel> validator, IPluginManagementService pluginManagementService) : base(validator)
        {
            _name = "My script";

            ScriptingProviders = new BindableCollection<ScriptingProvider>(pluginManagementService.GetFeaturesOfType<ScriptingProvider>());
            HasScriptingProviders = ScriptingProviders.Count > 0;
        }

        public BindableCollection<ScriptingProvider> ScriptingProviders { get; }
        public bool HasScriptingProviders { get; }

        public string Name
        {
            get => _name;
            set => SetAndNotify(ref _name, value);
        }

        public ScriptingProvider SelectedScriptingProvider
        {
            get => _selectedScriptingProvider;
            set => SetAndNotify(ref _selectedScriptingProvider, value);
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            Session.Close(new ScriptConfiguration(SelectedScriptingProvider, Name));
        }
    }

    public class ProfileElementScriptConfigurationCreateViewModelValidator : AbstractValidator<ScriptConfigurationCreateViewModel>
    {
        public ProfileElementScriptConfigurationCreateViewModelValidator()
        {
            RuleFor(m => m.Name).NotEmpty().WithMessage("Script name may not be empty");
            RuleFor(m => m.SelectedScriptingProvider).NotNull().WithMessage("Scripting provider is required");
        }
    }
}