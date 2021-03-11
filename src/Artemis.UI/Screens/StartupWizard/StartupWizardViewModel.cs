using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard.Steps;
using Artemis.UI.Shared.Services;
using MaterialDesignExtensions.Controllers;
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.StartupWizard
{
    public class StartupWizardViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IMessageService _messageService;
        private readonly ISettingsService _settingsService;
        private StepperController _stepperController;

        public StartupWizardViewModel(ISettingsService settingsService,
            IMessageService messageService,
            WelcomeStepViewModel welcome,
            DevicesStepViewModel devices,
            LayoutStepViewModel layout,
            SettingsStepViewModel settings,
            FinishStepViewModel finish)
        {
            _settingsService = settingsService;
            _messageService = messageService;
            Items.Add(welcome);
            Items.Add(devices);
            Items.Add(layout);
            Items.Add(settings);
            Items.Add(finish);

            ActiveItem = Items.First();
        }

        public ISnackbarMessageQueue MainMessageQueue { get; set; }

        public void ActiveStepChanged(object sender, ActiveStepChangedEventArgs e)
        {
            Stepper stepper = (Stepper) sender;
            _stepperController = stepper.Controller;

            int activeStepIndex = stepper.Steps.IndexOf(e.Step);
            ActiveItem = Items[activeStepIndex];
        }

        public void SkipOrFinishWizard()
        {
            PluginSetting<bool> setting = _settingsService.GetSetting("UI.SetupWizardCompleted", false);
            setting.Value = true;
            setting.Save();

            RequestClose();
        }

        public void Continue()
        {
            _stepperController.Continue();
        }

        protected override void OnInitialActivate()
        {
            MainMessageQueue = _messageService.MainMessageQueue;
            base.OnInitialActivate();
        }
    }
}