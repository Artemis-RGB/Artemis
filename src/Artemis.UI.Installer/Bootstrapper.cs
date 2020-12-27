using Artemis.UI.Installer.Screens;
using Artemis.UI.Installer.Screens.Steps;
using Artemis.UI.Installer.Services;
using Artemis.UI.Installer.Services.Prerequisites;
using Artemis.UI.Installer.Stylet;
using FluentValidation;
using Stylet;
using StyletIoC;

namespace Artemis.UI.Installer
{
    public class Bootstrapper : Bootstrapper<RootViewModel>
    {
        #region Overrides of Bootstrapper<RootViewModel>

        /// <inheritdoc />
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // View related stuff
            builder.Bind<ConfigurationStep>().ToAllImplementations();
            builder.Bind(typeof(IPrerequisite)).ToAllImplementations();
            
            // Services
            builder.Bind<IInstallationService>().To<InstallationService>().InSingletonScope();
            
            // Validation
            builder.Bind(typeof(IModelValidator<>)).To(typeof(FluentValidationAdapter<>));
            builder.Bind(typeof(IValidator<>)).ToAllImplementations();
            
            base.ConfigureIoC(builder);
        }

        #endregion
    }
}