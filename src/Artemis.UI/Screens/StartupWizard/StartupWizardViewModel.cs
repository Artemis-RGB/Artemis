using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard.Steps;
using MaterialDesignExtensions.Controllers;
using MaterialDesignExtensions.Controls;
using Stylet;

namespace Artemis.UI.Screens.StartupWizard
{
    public class StartupWizardViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly ISettingsService _settingsService;
        private StepperController _stepperController;

        public StartupWizardViewModel(ISettingsService settingsService,
            WelcomeStepViewModel welcome,
            DevicesStepViewModel devices,
            LayoutStepViewModel layout,
            SettingsStepViewModel settings,
            FinishStepViewModel finish)
        {
            _settingsService = settingsService;
            Items.Add(welcome);
            Items.Add(devices);
            Items.Add(layout);
            Items.Add(settings);
            Items.Add(finish);

            ActiveItem = Items.First();
        }

        public void ActiveStepChanged(object sender, ActiveStepChangedEventArgs e)
        {
            Stepper stepper = (Stepper) sender;
            _stepperController = stepper.Controller;

            int activeStepIndex = stepper.Steps.IndexOf(e.Step);
            ActiveItem = Items[activeStepIndex];
        }

        public void SkipOrFinishWizard()
        {
            RequestClose();
        }

        protected override void OnClose()
        {
            PluginSetting<bool> setting = _settingsService.GetSetting("UI.SetupWizardCompleted", false);
            setting.Value = true;
            setting.Save();

            base.OnClose();
        }
        
        public void Continue()
        {
            _stepperController.Continue();
        }
    }
}