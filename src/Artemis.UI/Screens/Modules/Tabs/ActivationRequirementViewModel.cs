using System.Timers;
using Artemis.Core.Modules;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.Modules.Tabs
{
    public class ActivationRequirementViewModel : Screen
    {
        private readonly IModuleActivationRequirement _activationRequirement;
        private readonly Timer _updateTimer;
        private string _requirementDescription;
        private bool _requirementMet;

        public ActivationRequirementViewModel(IModuleActivationRequirement activationRequirement)
        {
            _activationRequirement = activationRequirement;
            _updateTimer = new Timer(500);

            RequirementName = activationRequirement.GetType().Name.Humanize();
            RequirementDescription = activationRequirement.GetUserFriendlyDescription();

            _updateTimer.Elapsed += UpdateTimerOnElapsed;
        }

        public string RequirementName { get; }

        public string RequirementDescription
        {
            get => _requirementDescription;
            set => SetAndNotify(ref _requirementDescription, value);
        }

        public bool RequirementMet
        {
            get => _requirementMet;
            set => SetAndNotify(ref _requirementMet, value);
        }

        protected override void OnActivate()
        {
            Update();
            _updateTimer.Start();
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            _updateTimer.Stop();
            base.OnDeactivate();
        }

        private void UpdateTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            RequirementDescription = _activationRequirement.GetUserFriendlyDescription();
            RequirementMet = _activationRequirement.Evaluate();
        }
    }
}