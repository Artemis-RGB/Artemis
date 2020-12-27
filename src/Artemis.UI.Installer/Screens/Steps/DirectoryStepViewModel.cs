using System;
using System.IO;
using System.Threading.Tasks;
using Artemis.UI.Installer.Services;
using FluentValidation;
using Ookii.Dialogs.Wpf;
using Stylet;

namespace Artemis.UI.Installer.Screens.Steps
{
    public class DirectoryStepViewModel : ConfigurationStep
    {
        private readonly IInstallationService _installationService;
        private string _installationDirectory;

        public DirectoryStepViewModel(IInstallationService installationService, IModelValidator<DirectoryStepViewModel> validator) : base(validator)
        {
            _installationService = installationService;
        }

        public string InstallationDirectory
        {
            get => _installationDirectory;
            set
            {
                if (!SetAndNotify(ref _installationDirectory, value)) return;
                Validate();
            }
        }

        public override int Order => 3;

        public void BrowseDirectory()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog
            {
                Description = "Select the directory in which to install Artemis",
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() is bool accepted && accepted) 
                InstallationDirectory = Path.Combine(dialog.SelectedPath, "Artemis");
        }

        /// <inheritdoc />
        protected override void OnActivate()
        {
            InstallationDirectory = _installationService.InstallationDirectory;
            base.OnActivate();
        }

        /// <inheritdoc />
        protected override void OnDeactivate()
        {
            _installationService.InstallationDirectory = InstallationDirectory;
            base.OnDeactivate();
        }
    }

    public class DirectoryStepViewModelValidator : AbstractValidator<DirectoryStepViewModel>
    {
        public DirectoryStepViewModelValidator()
        {
            RuleFor(m => m.InstallationDirectory).NotEmpty().WithMessage("Installation directory is required");
            RuleFor(m => m.InstallationDirectory).Must(s =>
            {
                try
                {
                    bool rooted = Path.IsPathRooted(s);
                    string _ = Path.GetFullPath(s);
                    return rooted;
                }
                catch (Exception)
                {
                    return false;
                }
            }).WithMessage("Directory must be valid");
            RuleFor(m => m.InstallationDirectory).Must(s =>
            {
                try
                {
                    return Directory.Exists(Path.GetPathRoot(s));
                }
                catch (Exception)
                {
                    return false;
                }
            }).WithMessage("Directory is on an invalid drive");
        }
    }
}