using System.Collections.Generic;
using System.Linq;
using Artemis.UI.Installer.Screens.Steps;
using MaterialDesignExtensions.Controllers;
using MaterialDesignExtensions.Controls;
using Stylet;

namespace Artemis.UI.Installer.Screens
{
    public class RootViewModel : Conductor<ConfigurationStep>.Collection.OneActive
    {
        private StepperController _stepperController;

        public RootViewModel(IEnumerable<ConfigurationStep> configurationSteps)
        {
            Items.AddRange(configurationSteps.OrderBy(s => s.Order));
        }

        public void ActiveStepChanged(object sender, ActiveStepChangedEventArgs e)
        {
            Stepper stepper = (Stepper) sender;
            _stepperController = stepper.Controller;

            int activeStepIndex = stepper.Steps.IndexOf(e.Step);
            if (Items.Count > activeStepIndex)
                ActiveItem = Items[activeStepIndex];
            else
                _stepperController.Back();
        }

        public void Cancel()
        {
            RequestClose();
        }
    }
}